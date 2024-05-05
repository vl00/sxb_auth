using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using Enyim.Caching;
using iSchool.Authorization.Lib;
using iSchool.Authorization.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Console.Auth.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        [HttpGet]
        public IActionResult Login(string errorDescription)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            //Dictionary<string, string> acc = new Dictionary<string, string>();
            //acc.Add("CDPT300", "BDF25EAC");
            //string sql = "insert into admin_info values (@id, @name, @displayname, @password, @regTime, @loginTime, @activeTime, @ad, @rspw)";
            //foreach (var a in acc)
            //{
            //    connection.Execute(sql, new AdminInfo() { Id = Guid.NewGuid(), Name = a.Key, Displayname = a.Key, Password = MemcachedHelper.StrToMD5(a.Value), RegTime = DateTime.Today, Ad = false, Rspw=true });
            //}
            //return View();
            var provider = System.Security.Cryptography.RSA.Create();
            string publicKey = ToXmlString(provider, false);
            string privateKey = ToXmlString(provider, true);
            Guid kid = Guid.NewGuid();
            MemcachedHelper.Set(kid.ToString(), privateKey);
            ViewData["kid"] = kid;
            ViewData["publicKey"] = publicKey;
            ViewBag.ErrorDescription = errorDescription;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string kid, string account, string password, string redirect_uri)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            string privateKey = MemcachedHelper.Get(kid)?.ToString();
            if (string.IsNullOrEmpty(privateKey))
            {
                return Login("密钥已过期");
            }
            try
            {
                account = RSADecrypt(privateKey, account);
                password = RSADecrypt(privateKey, password);
            }
            catch
            {
                return Login("数据解密失败");
            }
            if(!DBAuth(out AdminInfo info, account, password))
            {
                if (!ADAuth(out info, account, password, out string Errordescription))
                {
                    return Login(Errordescription);
                }
            }
            if(info.Rspw)
            {
                return RedirectToAction("ChangePSW", new { errorDescription = "本次登录需先修改密码" });
            }
            RegAdmin(info);
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, info.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Hash, Guid.NewGuid().ToString()));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            MemcachedHelper.RemoveKeys(kid);

            return Redirect(string.IsNullOrEmpty(redirect_uri) ? "/" : redirect_uri);
        }
        private bool DBAuth(out AdminInfo info, string account, string password)
        {
            info = connection.QueryFirstOrDefault<AdminInfo>(@"select * from admin_info where name=@name and password=@password and ad=0", new { name = account, password = MemcachedHelper.StrToMD5(password) });
            return info != null;
        }
        private bool ADAuth(out AdminInfo info, string account, string password, out string ErrorDescription)
        {
            info = new AdminInfo();
            ErrorDescription = null;
            DirectoryEntry entry = new DirectoryEntry("LDAP://sxkid.com", account, password);
            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = "(samaccountname=" + account + ")";
            try
            {
                SearchResultCollection src = search.FindAll();
                foreach (SearchResult result in src)
                {
                    if (result != null)
                    {
                        //search.SearchScope = SearchScope.Base;
                        //search.Filter = @"(objectClass=*)";
                        //search.PropertiesToLoad.Add("maxPwdAge");
                        //SearchResult ouResult = search.FindOne();
                        //long maxPwdAge = 0;
                        //if (ouResult.Properties.Contains("maxPwdAge"))
                        //{
                        //    maxPwdAge = TimeSpan.FromTicks((long)ouResult.Properties["maxPwdAge"][0]).Days * -1;
                        //}

                        info.Id = Guid.Parse(BitConverter.ToString(result.Properties["objectguid"][0] as byte[]).Replace("-", ""));
                        info.Name = result.Properties["samaccountname"][0]?.ToString();
                        info.Displayname = result.Properties["displayname"][0]?.ToString();
                        info.Password = MemcachedHelper.StrToMD5(password);
                        info.Ad = true;
                        long.TryParse(result.Properties["pwdlastset"][0]?.ToString(), out long pwdlastset);
                        //if (pwdlastset > 0)
                        //{
                        //    double noChangePSWDay = (DateTime.UtcNow - new DateTime(pwdlastset).AddYears(1600)).TotalDays;
                        //    if (noChangePSWDay > maxPwdAge)
                        //    {
                        //        ErrorDescription = "密码已过期";
                        //        return false;
                        //    }
                        //    else
                        //    {
                        //        //info.statusStr += "密码有效期：" + Math.Floor(maxPwdAge - noChangePSWDay) + "天;";
                        //    }
                        //}
                        //else
                        //{
                        //    ErrorDescription = "密码已过期";
                        //    return false;
                        //}
                        string useraccountcontrol = Convert.ToString(Convert.ToInt32(result.Properties["useraccountcontrol"][0]), 2).PadLeft(25, '0');
                        if (useraccountcontrol.ElementAt(23) == '1')
                        {
                            ErrorDescription = "用户帐户已禁用";
                            return false;
                        }
                        if (useraccountcontrol.ElementAt(20) == '1')
                        {
                            ErrorDescription = "该帐户目前已被锁定";
                            return false;
                        }
                        if (useraccountcontrol.ElementAt(19) == '1')
                        {
                            //ViewBag.ErrorDescription = "无需密码";
                        }
                        if (useraccountcontrol.ElementAt(18) == '1')
                        {
                            //info.statusStr += "用户无法更改密码;";
                        }
                        if (useraccountcontrol.ElementAt(15) == '1')
                        {
                            //info.statusStr += "这是代表典型用户的默认帐户类型;";
                        }
                        if (useraccountcontrol.ElementAt(8) == '1')
                        {
                            //info.statusStr += "此帐户的密码永不过期;";
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorDescription = ex.Message;
                return false;
            }
        }
        private bool ADChangePSW(string account, string password, string password_n, out string ErrorDescription)
        {
            ErrorDescription = string.Empty;
            DirectoryEntry entry = new DirectoryEntry("LDAP://sxkid.com", account, password, AuthenticationTypes.Secure);
            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = "(samaccountname=" + account + ")";
            try
            {
                SearchResult result = search.FindOne();
                DirectoryEntry userEntry = result.GetDirectoryEntry();
                userEntry.Invoke("ChangePassword", new object[] { password, password_n });
                entry.CommitChanges();//提交修改 
                return true;
            }
            catch (DirectoryServicesCOMException ex)
            {
                ErrorDescription = ex.Message;
                return false;
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                ErrorDescription = ex.InnerException.Message;
                return false;
            }
            catch (Exception ex)
            {
                ErrorDescription = ex.Message;
                return false;
            }
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }
        [HttpGet]
        public IActionResult ChangePSW(string errorDescription)
        {
            var provider = System.Security.Cryptography.RSA.Create();
            string publicKey = ToXmlString(provider, false);
            string privateKey = ToXmlString(provider, true);
            Guid kid = Guid.NewGuid();
            MemcachedHelper.Set(kid.ToString(), privateKey);
            ViewData["kid"] = kid;
            ViewData["publicKey"] = publicKey;
            ViewBag.ErrorDescription = errorDescription;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePSW(string kid, string account, string password, string password_n)
        {
            string privateKey = MemcachedHelper.Get(kid)?.ToString();
            if (string.IsNullOrEmpty(privateKey))
            {
                return ChangePSW("密钥已过期");
            }
            try
            {
                account = RSADecrypt(privateKey, account);
                password = RSADecrypt(privateKey, password);
                password_n = RSADecrypt(privateKey, password_n);
            }
            catch
            {
                return ChangePSW("数据解密失败");
            }

            if (password == password_n)
            {
                return ChangePSW("新旧密码不能一样");
            }

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[a-zA-Z]+\d+|\d+[a-zA-Z]+");
            if (password_n.Length < 8 || !regex.IsMatch(password_n))
            {
                return ChangePSW("新密码必须包含数字和字母且最少8个字符");
            }

            if(DBAuth(out AdminInfo info, account, password))
            {
                if (connection.Execute("update admin_info set password=@password_n, loginTime=@loginTime, rspw=0 where id=@id", new { id = info.Id, password_n = MemcachedHelper.StrToMD5(password_n), loginTime=DateTime.Now }) > 0)
                {
                    return await Logout();
                }
                return ChangePSW("修改密码失败，请联系管理员");
            }
            if(!ADChangePSW(account, password, password_n, out string Errordescription))
            {
                return ChangePSW(Errordescription);
            }
            return await Logout();
        }



        public void RegAdmin(AdminInfo model)
        {
            if (Convert.ToInt32(connection.ExecuteScalar("select count(*) from admin_info where id=@id", model)) == 0)
            {
                model.RegTime = model.LoginTime = model.ActiveTime = DateTime.Now;
                connection.Execute("insert into admin_info (id, name, displayname, password, regTime, loginTime, activeTime, rspw) values (@id, @name, @displayname, @password, @regTime, @loginTime, @activeTime, 0)", model);
            }
            else
            {
                model.LoginTime = model.ActiveTime = DateTime.Now;
                connection.Execute("update admin_info set name=@name, displayname=@displayname, password=@password, loginTime=@loginTime, activeTime=@activeTime where id=@id", model);
            }
            connection.Close();
        }

        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publickey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string publickey, string content)
        {
            var rsa = System.Security.Cryptography.RSA.Create();
            byte[] cipherbytes;
            FromXmlString(rsa, publickey);
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), RSAEncryptionPadding.Pkcs1);

            return Convert.ToBase64String(cipherbytes);
        }
        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="privatekey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSADecrypt(string privatekey, string content)
        {
            var rsa = System.Security.Cryptography.RSA.Create();
            byte[] cipherbytes;
            FromXmlString(rsa, privatekey);
            cipherbytes = rsa.Decrypt(Convert.FromBase64String(content), RSAEncryptionPadding.Pkcs1);

            return Encoding.UTF8.GetString(cipherbytes);
        }
        public static void FromXmlString(RSA rsa, string xmlString)
        {
            RSAParameters parameters = new RSAParameters();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus": parameters.Modulus = Convert.FromBase64String(node.InnerText); break;
                        case "Exponent": parameters.Exponent = Convert.FromBase64String(node.InnerText); break;
                        case "P": parameters.P = Convert.FromBase64String(node.InnerText); break;
                        case "Q": parameters.Q = Convert.FromBase64String(node.InnerText); break;
                        case "DP": parameters.DP = Convert.FromBase64String(node.InnerText); break;
                        case "DQ": parameters.DQ = Convert.FromBase64String(node.InnerText); break;
                        case "InverseQ": parameters.InverseQ = Convert.FromBase64String(node.InnerText); break;
                        case "D": parameters.D = Convert.FromBase64String(node.InnerText); break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            rsa.ImportParameters(parameters);
        }

        public static string ToXmlString(RSA rsa, bool includePrivateParameters)
        {
            RSAParameters parameters = rsa.ExportParameters(includePrivateParameters);

            if (includePrivateParameters)
            {
                return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                    Convert.ToBase64String(parameters.Modulus),
                    Convert.ToBase64String(parameters.Exponent),
                    Convert.ToBase64String(parameters.P),
                    Convert.ToBase64String(parameters.Q),
                    Convert.ToBase64String(parameters.DP),
                    Convert.ToBase64String(parameters.DQ),
                    Convert.ToBase64String(parameters.InverseQ),
                    Convert.ToBase64String(parameters.D));
            }
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                    Convert.ToBase64String(parameters.Modulus),
                    Convert.ToBase64String(parameters.Exponent));
        }
    }
}