using iSchool.Sites.UserCenter.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.BLL
{
    public class HistoryBLL
    {
        private DAL.HistoryDAL historyDAL = new DAL.HistoryDAL();
        public bool AddHistory(Guid userID, Guid dataID, byte dataType)
        {
            return historyDAL.AddHistory(userID, dataID, dataType);
        }
        public bool RemoveHistory(Guid userID, Guid dataID)
        {
            return historyDAL.RemoveHistory(userID, dataID);
        }

        public List<Guid> GetUserHistory(Guid userID, byte dataType, int page)
        {
            return historyDAL.GetUserHistory(userID, dataType, page, 10);
        }
        public List<Models.Vo.SchoolVo.SchoolModel> GetSchoolHistory(Guid userID, double? lat = null, double? lng = null, int page = 1)
        {
            var iDList = GetUserHistory(userID, 1, page);
            var list = HttpHelper.HttpPost<List<Models.Vo.SchoolVo.SchoolModel>>(
                "https://www.sxkid.com/School/GetHistoryExtAsync", Newtonsoft.Json.JsonConvert.SerializeObject(iDList));
            foreach (var school in list)
            {
                if (school.Tuition != null)
                {
                    school.Tuition = Math.Round(school.Tuition.Value / 10000, 2);
                }
                if (lat != null && lng != null && school.Latitude != null && school.Longitude != null)
                {
                    school.Distance = CommonHelper.FormatDistance(CommonHelper.GetDistance(lat.Value, lng.Value, school.Latitude.Value, school.Longitude.Value));
                }
            }
            return list;
        }
        public List<Models.Vo.CommentVo.CommentModel> GetCommentHistory(Guid userID, int page = 1)
        {
            var iDList = GetUserHistory(userID, 3, page);
            var list = HttpHelper.HttpPost<List<Models.Vo.CommentVo.CommentModel>>(
                "https://www.sxkid.com/School/GetHistoryExtAsync", Newtonsoft.Json.JsonConvert.SerializeObject(iDList));
            return list;
        }
        public List<Models.Vo.CommentVo.CommentModel> GetQAHistory(Guid userID, int page = 1)
        {
            var iDList = GetUserHistory(userID, 2, page);
            var list = HttpHelper.HttpPost<List<Models.Vo.CommentVo.CommentModel>>(
                "https://www.sxkid.com/School/GetHistoryExtAsync", Newtonsoft.Json.JsonConvert.SerializeObject(iDList));
            return list;
        }
        public List<Models.Vo.QuestionVo.QuestionModel> GetArticleHistory(Guid userID, int page = 1)
        {
            var iDList = GetUserHistory(userID, 0, page);
            var list = HttpHelper.HttpPost<List<Models.Vo.QuestionVo.QuestionModel>>(
                "https://operation.sxkid.com/api/ArticleApi/GetArticles", Newtonsoft.Json.JsonConvert.SerializeObject(iDList));
            return list;
        }
    }
}
