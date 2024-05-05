using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Library
{
    public class RndCodeModel
    {
        public string Mobile { get; set; }
        public string Code { get; set; }
        public string CodeType { get; set; }
        public DateTime CodeTime { get; set; }
    }
    public class SMSHelper
    {
        public bool SendRndCode(string mobile, string codeType, short nationCode, out string failReason)
        {
            if (!string.IsNullOrEmpty(mobile))
            {
                RndCodeModel CodeCache = RedisHelper.Get<RndCodeModel>("RNDCode-" + nationCode + mobile + "-" + codeType);
                if (CodeCache != null && CodeCache.CodeTime > DateTime.Now.AddMinutes(-1))
                {
                    failReason = "发送太频繁了，请稍后再试";
                    return false;
                }
                int sendCount = RedisHelper.Get<int>("RNDCodeSendCount-" + nationCode + mobile + "-" + codeType);
                if (sendCount >= 5)
                {
                    failReason = "此号码今天短信验证码发送的次数已超过限制，请改用其他验证方式";
                    return false;
                }

                int rndCode = new Random().Next(0, 999999);

                string codeStr = rndCode.ToString().PadLeft(6, '0');
                var sendResult = SendSMS(mobile, 10834, new List<string>() { codeStr }, nationCode);
                if (sendResult.result == 0)
                {
                    RedisHelper.Set("RNDCode-" + nationCode + mobile + "-" + codeType, new RndCodeModel() {
                        Mobile = nationCode + mobile,
                        Code = codeStr,
                        CodeType = codeType,
                        CodeTime = DateTime.Now
                    }, 300);
                    RedisHelper.Set("RNDCodeSendCount-" + nationCode + mobile + "-" + codeType, ++sendCount, DateTime.Today.AddDays(1));
                    failReason = null;
                    return true;
                }
                else
                {
                    failReason = sendResult.errmsg;
                    return false;
                }
            }
            else
            {
                failReason = "请输入有效的手机号码";
                return false;
            }
        }
        public bool CheckRndCode(short nationCode, string mobile, string code, string codeType, bool removeCache=true)
        {
            RndCodeModel CodeCache = RedisHelper.Get<RndCodeModel>("RNDCode-" + nationCode + mobile + "-" + codeType);
            if(CodeCache != null && CodeCache.Code == code && CodeCache.CodeType == codeType)
            {
                if (removeCache)
                {
                    RedisHelper.Remove("RNDCode-" + nationCode + mobile + "-" + codeType);
                }
                return true;
            }
            return false;
        }
        public SmsSingleSenderResult SendSMS(string mobile, int templID, List<string> templ, short nationCode = 86, int sdkappid = 1400013556, string appkey = "ba4604e9bba557e6792c876c5e609df2", string sign = null)
        {
            TXSMSHelper smsHelper = new TXSMSHelper();
            SmsSingleSenderResult res = smsHelper.SendWithParam(sdkappid, appkey, nationCode.ToString(), mobile, templID, templ, sign ?? "", "", "");
            if (res.result == 0)
            {
                SmsTemplateHandler handler = new SmsTemplateHandler();
                SmsTemplateListResult templateResult = handler.GetTemplate(sdkappid, appkey, templID);
                if (templateResult.result == 0)
                {
                    templ.Insert(0, "");
                    string SMSContent = string.Format("{0}" + templateResult.data[0].text, templ.ToArray());
                    //AddSMSRecord(new Model.Entity.dbo.sms_record()
                    //{
                    //    id = Guid.NewGuid(),
                    //    sid = res.sid,
                    //    mobile = mobile,
                    //    status = "发送中",
                    //    text = SMSContent,
                    //    time = DateTime.Now,
                    //    type = Model.Custom.SMSRecordType.系统发送
                    //});
                }
            }
            return res;
        }
        //public bool AddSMSRecord(Model.Entity.dbo.sms_record model)
        //{
        //    return SMS_DAL.AddSMSRecord(model);
        //}
    }
    #region 腾讯云短信SDK
    public class TXSMSHelper
    {
        //int sdkappid = 1400013556;
        //string appkey = "ba4604e9bba557e6792c876c5e609df2";
        string url = "https://yun.tim.qq.com/v5/tlssmssvr/sendsms";

        SmsSenderUtil util = new SmsSenderUtil();

        public TXSMSHelper() { }

        /**
         * 普通单发短信接口，明确指定内容，如果有多个签名，请在内容中以【】的方式添加到信息内容中，否则系统将使用默认签名
         * @param type 短信类型，0 为普通短信，1 营销短信
         * @param nationCode 国家码，如 86 为中国
         * @param phoneNumber 不带国家码的手机号
         * @param msg 信息内容，必须与申请的模板格式一致，否则将返回错误
         * @param extend 扩展码，可填空
         * @param ext 服务端原样返回的参数，可填空
         * @return SmsSingleSenderResult
         */
        public SmsSingleSenderResult Send(
            int sdkappid,
            string appkey,
            int type,
            string nationCode,
            string phoneNumber,
            string msg,
            string extend,
            string ext)
        {
            /*
            请求包体
            {
                "tel": {
                    "nationcode": "86", 
                    "mobile": "13788888888"
                },
                "type": 0, 
                "msg": "你的验证码是1234", 
                "sig": "fdba654e05bc0d15796713a1a1a2318c", 
                "time": 1479888540,
                "extend": "",
                "ext": ""
            }
            应答包体
            {
                "result": 0,
                "errmsg": "OK", 
                "ext": "", 
                "sid": "xxxxxxx", 
                "fee": 1
            }
            */
            if (0 != type && 1 != type)
            {
                throw new Exception("type " + type + " error");
            }
            if (null == extend)
            {
                extend = "";
            }
            if (null == ext)
            {
                ext = "";
            }

            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();

            JObject tel = new JObject();
            tel.Add("nationcode", nationCode);
            tel.Add("mobile", phoneNumber);

            data.Add("tel", tel);
            data.Add("msg", msg);
            data.Add("type", type);
            data.Add("sig", util.StrToHash(String.Format(
                "appkey={0}&random={1}&time={2}&mobile={3}",
                appkey, random, curTime, phoneNumber)));
            data.Add("time", curTime);
            data.Add("extend", extend);
            data.Add("ext", ext);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsSingleSenderResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToSingleSenderResult(responseStr);
            }
            else
            {
                result = new SmsSingleSenderResult();
                result.result = -1;
                result.errmsg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }

        /**
         * 指定模板单发
         * @param nationCode 国家码，如 86 为中国
         * @param phoneNumber 不带国家码的手机号
         * @param templId 模板 id
         * @param templParams 模板参数列表，如模板 {1}...{2}...{3}，那么需要带三个参数
         * @param extend 扩展码，可填空
         * @param ext 服务端原样返回的参数，可填空
         * @return SmsSingleSenderResult
         */
        public SmsSingleSenderResult SendWithParam(
            int sdkappid,
            string appkey,
            string nationCode,
            string phoneNumber,
            int templId,
            List<string> templParams,
            string sign,
            string extend,
            string ext)
        {
            /*
            请求包体
            {
                "tel": {
                    "nationcode": "86",
                    "mobile": "13788888888"
                },
                "sign": "腾讯云",
                "tpl_id": 19,
                "params": [
                    "验证码", 
                    "1234",
                    "4"
                ],
                "sig": "fdba654e05bc0d15796713a1a1a2318c",
                "time": 1479888540,
                "extend": "",
                "ext": ""
            }
            应答包体
            {
                "result": 0,
                "errmsg": "OK", 
                "ext": "", 
                "sid": "xxxxxxx", 
                "fee": 1
            }
            */
            if (null == sign)
            {
                sign = "";
            }
            if (null == extend)
            {
                extend = "";
            }
            if (null == ext)
            {
                ext = "";
            }

            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();

            JObject tel = new JObject();
            tel.Add("nationcode", nationCode);
            tel.Add("mobile", phoneNumber);

            data.Add("tel", tel);
            data.Add("sig", util.CalculateSigForTempl(appkey, random, curTime, phoneNumber));
            data.Add("tpl_id", templId);
            data.Add("params", util.SmsParamsToJSONArray(templParams));
            data.Add("sign", sign);
            data.Add("time", curTime);
            data.Add("extend", extend);
            data.Add("ext", ext);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsSingleSenderResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToSingleSenderResult(responseStr);
            }
            else
            {
                result = new SmsSingleSenderResult();
                result.result = -1;
                result.errmsg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }
    }

    public class SmsSingleSenderResult
    {
        /*
        {
            "result": 0,
            "errmsg": "OK", 
            "ext": "", 
            "sid": "xxxxxxx", 
            "fee": 1
        }
         */
        public int result { set; get; }
        public string errmsg { set; get; }
        public string ext { set; get; }
        public string sid { set; get; }
        public int fee { set; get; }

        public override string ToString()
        {
            return string.Format(
                "SmsSingleSenderResult\nresult {0}\nerrMsg {1}\next {2}\nsid {3}\nfee {4}",
                result, errmsg, ext, sid, fee);
        }
    }

    public class TXSMSMultiSender
    {
        //int sdkappid = 1400013556;
        //string appkey = "ba4604e9bba557e6792c876c5e609df2";
        string url = "https://yun.tim.qq.com/v5/tlssmssvr/sendmultisms2";

        SmsSenderUtil util = new SmsSenderUtil();

        public TXSMSMultiSender() { }

        /**
         * 普通群发短信接口，明确指定内容，如果有多个签名，请在内容中以【】的方式添加到信息内容中，否则系统将使用默认签名
         * 【注意】海外短信无群发功能
         * @param type 短信类型，0 为普通短信，1 营销短信
         * @param nationCode 国家码，如 86 为中国
         * @param phoneNumbers 不带国家码的手机号列表
         * @param msg 信息内容，必须与申请的模板格式一致，否则将返回错误
         * @param extend 扩展码，可填空
         * @param ext 服务端原样返回的参数，可填空
         * @return SmsMultiSenderResult
         */
        public SmsMultiSenderResult Send(
            int sdkappid,
            string appkey,
            int type,
            string nationCode,
            List<string> phoneNumbers,
            string msg,
            string extend,
            string ext)
        {
            /*
            请求包体
            {
                "tel": [
                    {
                        "nationcode": "86", 
                        "mobile": "13788888888"
                    }, 
                    {
                        "nationcode": "86", 
                        "mobile": "13788888889"
                    }
                ], 
                "type": 0, 
                "msg": "你的验证码是1234", 
                "sig": "fdba654e05bc0d15796713a1a1a2318c",
                "time": 1479888540,
                "extend": "", 
                "ext": ""
            }
            应答包体
            {
                "result": 0, 
                "errmsg": "OK", 
                "ext": "", 
                "detail": [
                    {
                        "result": 0, 
                        "errmsg": "OK", 
                        "mobile": "13788888888", 
                        "nationcode": "86", 
                        "sid": "xxxxxxx", 
                        "fee": 1
                    }, 
                    {
                        "result": 0, 
                        "errmsg": "OK", 
                        "mobile": "13788888889", 
                        "nationcode": "86", 
                        "sid": "xxxxxxx", 
                        "fee": 1
                    }
                ]
            }
            */
            if (0 != type && 1 != type)
            {
                throw new Exception("type " + type + " error");
            }
            if (null == extend)
            {
                extend = "";
            }
            if (null == ext)
            {
                ext = "";
            }

            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();
            data.Add("tel", util.PhoneNumbersToJSONArray(nationCode, phoneNumbers));
            data.Add("type", type);
            data.Add("msg", msg);
            data.Add("sig", util.CalculateSig(appkey, random, curTime, phoneNumbers));
            data.Add("time", curTime);
            data.Add("extend", extend);
            data.Add("ext", ext);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsMultiSenderResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToMultiSenderResult(responseStr);
            }
            else
            {
                result = new SmsMultiSenderResult();
                result.result = -1;
                result.errmsg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }

        /**
         * 指定模板群发
         * 【注意】海外短信无群发功能
         * @param nationCode 国家码，如 86 为中国
         * @param phoneNumbers 不带国家码的手机号列表
         * @param templId 模板 id
         * @param params 模板参数列表
         * @param sign 签名，如果填空，系统会使用默认签名
         * @param extend 扩展码，可以填空
         * @param ext 服务端原样返回的参数，可以填空
         * @return SmsMultiSenderResult
         */
        public SmsMultiSenderResult SendWithParam(
            int sdkappid,
            string appkey,
            String nationCode,
            List<string> phoneNumbers,
            int templId,
            List<string> templParams,
            string sign,
            string extend,
            string ext)
        {
            /*
            请求包体
            {
                "tel": [
                    {
                        "nationcode": "86", 
                        "mobile": "13788888888"
                    }, 
                    {
                        "nationcode": "86", 
                        "mobile": "13788888889"
                    }
                ], 
                "type": 0, 
                "msg": "你的验证码是1234", 
                "sig": "fdba654e05bc0d15796713a1a1a2318c",
                "time": 1479888540,
                "extend": "", 
                "ext": ""
            }
            应答包体
            {
                "result": 0, 
                "errmsg": "OK", 
                "ext": "", 
                "detail": [
                    {
                        "result": 0, 
                        "errmsg": "OK", 
                        "mobile": "13788888888", 
                        "nationcode": "86", 
                        "sid": "xxxxxxx", 
                        "fee": 1
                    }, 
                    {
                        "result": 0, 
                        "errmsg": "OK", 
                        "mobile": "13788888889", 
                        "nationcode": "86", 
                        "sid": "xxxxxxx", 
                        "fee": 1
                    }
                ]
            }
            */
            if (null == sign)
            {
                sign = "";
            }
            if (null == extend)
            {
                extend = "";
            }
            if (null == ext)
            {
                ext = "";
            }

            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();
            data.Add("tel", util.PhoneNumbersToJSONArray(nationCode, phoneNumbers));
            data.Add("sig", util.CalculateSigForTempl(appkey, random, curTime, phoneNumbers));
            data.Add("tpl_id", templId);
            data.Add("params", util.SmsParamsToJSONArray(templParams));
            data.Add("sign", sign);
            data.Add("time", curTime);
            data.Add("extend", extend);
            data.Add("ext", ext);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsMultiSenderResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToMultiSenderResult(responseStr);
            }
            else
            {
                result = new SmsMultiSenderResult();
                result.result = -1;
                result.errmsg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }
    }

    public class SmsMultiSenderResult
    {
        /*
        {
            "result": 0, 
            "errmsg": "OK", 
            "ext": "", 
            "detail": [
                {
                    "result": 0, 
                    "errmsg": "OK", 
                    "mobile": "13788888888", 
                    "nationcode": "86", 
                    "sid": "xxxxxxx", 
                    "fee": 1
                }, 
                {
                    "result": 0, 
                    "errmsg": "OK", 
                    "mobile": "13788888889", 
                    "nationcode": "86", 
                    "sid": "xxxxxxx", 
                    "fee": 1
                }
            ]
        }
            */
        public class Detail
        {
            public int result { get; set; }
            public string errmsg { get; set; }
            public string mobile { get; set; }
            public string nationcode { get; set; }
            public string sid { get; set; }
            public int fee { get; set; }

            public override string ToString()
            {
                return string.Format(
                        "\tDetail result {0} errmsg {1} mobile {2} nationcode {3} sid {4} fee {5}",
                        result, errmsg, mobile, nationcode, sid, fee);
            }
        }

        public int result;
        public string errmsg = "";
        public string ext = "";
        public IList<Detail> detail;

        public override string ToString()
        {
            if (null != detail)
            {
                return String.Format(
                        "SmsMultiSenderResult\nresult {0}\nerrmsg {1}\next {2}\ndetail:\n{3}",
                        result, errmsg, ext, String.Join("\n", detail));
            }
            else
            {
                return String.Format(
                     "SmsMultiSenderResult\nresult {0}\nerrmsg {1}\next {2}\n",
                     result, errmsg, ext);
            }
        }
    }
    public class SmsTemplateHandler
    {
        //int sdkappid = 1400013556;
        //string appkey = "ba4604e9bba557e6792c876c5e609df2";
        SmsSenderUtil util = new SmsSenderUtil();
        public SmsTemplateResult AddTemplate(int sdkappid, string appkey, string title, string text, string remark, byte type)
        {
            string url = "https://yun.tim.qq.com/v5/tlssmssvr/add_template";
            if (0 != type && 1 != type && 2 != type)
            {
                throw new Exception("type " + type + " error");
            }

            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();

            data.Add("title", title);
            data.Add("remark", remark);
            data.Add("text", text);
            data.Add("type", type);
            data.Add("sig", util.StrToHash(String.Format(
                "appkey={0}&random={1}&time={2}",
                appkey, random, curTime)));
            data.Add("time", curTime);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsTemplateResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToResult<SmsTemplateResult>(responseStr);
            }
            else
            {
                result = new SmsTemplateResult();
                result.result = -1;
                result.msg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }
        public SmsTemplateResult UpdateTemplate(int sdkappid, string appkey, int tpl_id, string title, string text, string remark, int type)
        {
            string url = "https://yun.tim.qq.com/v5/tlssmssvr/mod_template";
            if (0 != type && 1 != type && 2 != type)
            {
                throw new Exception("type " + type + " error");
            }

            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();

            data.Add("tpl_id", tpl_id);
            data.Add("title", title);
            data.Add("remark", remark);
            data.Add("text", text);
            data.Add("type", type);
            data.Add("sig", util.StrToHash(String.Format(
                "appkey={0}&random={1}&time={2}",
                appkey, random, curTime)));
            data.Add("time", curTime);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsTemplateResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToResult<SmsTemplateResult>(responseStr);
            }
            else
            {
                result = new SmsTemplateResult();
                result.result = -1;
                result.msg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }
        public SmsTemplateResult DeleteTemplate(int sdkappid, string appkey, int tpl_id)
        {
            string url = "https://yun.tim.qq.com/v5/tlssmssvr/del_template";
            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();

            data.Add("tpl_id", tpl_id);
            data.Add("sig", util.StrToHash(String.Format(
                "appkey={0}&random={1}&time={2}",
                appkey, random, curTime)));
            data.Add("time", curTime);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsTemplateResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToResult<SmsTemplateResult>(responseStr);
            }
            else
            {
                result = new SmsTemplateResult();
                result.result = -1;
                result.msg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }
        public SmsTemplateListResult GetTemplate(int sdkappid, string appkey, int tpl_id)
        {
            string url = "https://yun.tim.qq.com/v5/tlssmssvr/get_template";
            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();
            JArray tpl_id_array = JArray.Parse("[" + tpl_id + "]");
            data.Add("tpl_id", tpl_id_array);
            data.Add("sig", util.StrToHash(String.Format(
                "appkey={0}&random={1}&time={2}",
                appkey, random, curTime)));
            data.Add("time", curTime);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsTemplateListResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToResult<SmsTemplateListResult>(responseStr);
            }
            else
            {
                result = new SmsTemplateListResult();
                result.result = -1;
                result.msg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }
        public SmsTemplateListResult GetTemplateList(int sdkappid, string appkey, int page)
        {
            string url = "https://yun.tim.qq.com/v5/tlssmssvr/get_template";
            long random = util.GetRandom();
            long curTime = util.GetCurTime();

            // 按照协议组织 post 请求包体
            JObject data = new JObject();
            JObject tpl_page = new JObject();
            tpl_page.Add("offset", (page - 1) * 50);
            tpl_page.Add("max", 50);

            data.Add("tpl_page", tpl_page);
            data.Add("sig", util.StrToHash(String.Format(
                "appkey={0}&random={1}&time={2}",
                appkey, random, curTime)));
            data.Add("time", curTime);

            string wholeUrl = url + "?sdkappid=" + sdkappid + "&random=" + random;
            HttpWebRequest request = util.GetPostHttpConn(wholeUrl);
            byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            request.ContentLength = requestData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestData, 0, requestData.Length);
            requestStream.Close();

            // 接收返回包
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseStr = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            SmsTemplateListResult result;
            if (HttpStatusCode.OK == response.StatusCode)
            {
                result = util.ResponseStrToResult<SmsTemplateListResult>(responseStr);
            }
            else
            {
                result = new SmsTemplateListResult();
                result.result = -1;
                result.msg = "http error " + response.StatusCode + " " + responseStr;
            }
            return result;
        }
    }
    public class SmsTemplateResult
    {
        public class Data
        {
            public int id { get; set; }
            public string text { get; set; }
            public byte status { get; set; }
            public byte type { get; set; }
        }
        public int result { get; set; }
        public string msg { get; set; }
        public Data data { get; set; }
    }
    public class SmsTemplateListResult
    {
        public class Data
        {
            public int id { get; set; }
            public string title { get; set; }
            public string text { get; set; }
            public byte status { get; set; }
            public string reply { get; set; }
            public byte type { get; set; }
            public DateTime apply_time { get; set; }
        }
        public int result { get; set; }
        public string msg { get; set; }
        public int total { get; set; }
        public int count { get; set; }
        public IList<Data> data { get; set; }
    }

    class SmsSenderUtil
    {
        Random random = new Random();

        public HttpWebRequest GetPostHttpConn(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        public long GetRandom()
        {
            return random.Next(999999) % 900000 + 100000;
        }

        public long GetCurTime()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }

        // 将二进制的数值转换为 16 进制字符串，如 "abc" => "616263"
        private static string ByteArrayToHex(byte[] byteArray)
        {
            string returnStr = "";
            if (byteArray != null)
            {
                for (int i = 0; i < byteArray.Length; i++)
                {
                    returnStr += byteArray[i].ToString("x2");
                }
            }
            return returnStr;
        }

        public string StrToHash(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] resultByteArray = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
            return ByteArrayToHex(resultByteArray);
        }

        // 将单发回包解析成结果对象
        public SmsSingleSenderResult ResponseStrToSingleSenderResult(string str)
        {
            SmsSingleSenderResult result = JsonConvert.DeserializeObject<SmsSingleSenderResult>(str);
            return result;
        }

        // 将群发回包解析成结果对象
        public SmsMultiSenderResult ResponseStrToMultiSenderResult(string str)
        {
            SmsMultiSenderResult result = JsonConvert.DeserializeObject<SmsMultiSenderResult>(str);
            return result;
        }
        public T ResponseStrToResult<T>(string str)
        {
            T result = JsonConvert.DeserializeObject<T>(str);
            return result;
        }

        public JArray SmsParamsToJSONArray(List<string> templParams)
        {
            JArray smsParams = new JArray();
            foreach (string templParamsElement in templParams)
            {
                smsParams.Add(templParamsElement);
            }
            return smsParams;
        }

        public JArray PhoneNumbersToJSONArray(string nationCode, List<string> phoneNumbers)
        {
            JArray tel = new JArray();
            int i = 0;
            do
            {
                JObject telElement = new JObject();
                telElement.Add("nationcode", nationCode);
                telElement.Add("mobile", phoneNumbers.ElementAt(i));
                tel.Add(telElement);
            } while (++i < phoneNumbers.Count);

            return tel;
        }

        public string CalculateSigForTempl(
            string appkey,
            long random,
            long curTime,
            List<string> phoneNumbers)
        {
            string phoneNumbersString = phoneNumbers.ElementAt(0);
            for (int i = 1; i < phoneNumbers.Count; i++)
            {
                phoneNumbersString += "," + phoneNumbers.ElementAt(i);
            }
            return StrToHash(String.Format(
                "appkey={0}&random={1}&time={2}&mobile={3}",
                appkey, random, curTime, phoneNumbersString));
        }

        public string CalculateSigForTempl(
            string appkey,
            long random,
            long curTime,
            string phoneNumber)
        {
            List<string> phoneNumbers = new List<string>();
            phoneNumbers.Add(phoneNumber);
            return CalculateSigForTempl(appkey, random, curTime, phoneNumbers);
        }

        public string CalculateSig(
            string appkey,
            long random,
            long curTime,
            List<string> phoneNumbers)
        {
            string phoneNumbersString = phoneNumbers.ElementAt(0);
            for (int i = 1; i < phoneNumbers.Count; i++)
            {
                phoneNumbersString += "," + phoneNumbers.ElementAt(i);
            }
            return StrToHash(String.Format(
                    "appkey={0}&random={1}&time={2}&mobile={3}",
                    appkey, random, curTime, phoneNumbersString));
        }
    }
    class SendMessageRequest
    {
        public SendMessageRequest() { }
        public string signature_id { get; set; }
        public string mobile { get; set; }
        public string template_id { get; set; }
        public Dictionary<string, string> context { get; set; }
    }
    #endregion
}
