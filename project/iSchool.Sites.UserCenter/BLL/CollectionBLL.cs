using iSchool.Sites.UserCenter.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.BLL
{
    public class CollectionBLL
    {
        private DAL.CollectionDAL collectionDAL = new DAL.CollectionDAL();
        public bool AddCollection(Guid userID, Guid dataID, byte dataType)
        {
            return collectionDAL.AddCollection(userID, dataID, dataType);
        }
        public bool RemoveCollection(Guid userID, Guid dataID)
        {
            return collectionDAL.RemoveCollection(userID, dataID);
        }
        public bool IsCollected(Guid userID, Guid dataID)
        {
            return collectionDAL.IsCollected(userID, dataID);
        }

        public List<Guid> GetUserCollection(Guid userID, byte dataType, int page)
        {
            return collectionDAL.GetUserCollection(userID, dataType, page, 10);
        }
        public List<Models.ViewsModels.Collection.School> GetSchoolCollection(Guid userID, double? lat=null, double? lng=null, int page=1)
        {
            var iDList = GetUserCollection(userID, 1, page);
            var list = HttpHelper.HttpPost<List<Models.ViewsModels.Collection.School>>(
                "https://www.sxkid.com/School/GetCollectionExtAsync", Newtonsoft.Json.JsonConvert.SerializeObject(iDList));
            foreach(var school in list)
            {
                if (school.Tuition != null)
                {
                    school.Tuition = Math.Round(school.Tuition.Value / 10000, 2);
                }
                if(lat!=null && lng!=null && school.Latitude!=null && school.Longitude != null)
                {
                    school.Distance = CommonHelper.FormatDistance(CommonHelper.GetDistance(lat.Value, lng.Value, school.Latitude.Value, school.Longitude.Value));
                }
            }
            return list;
        }
        public List<Models.Vo.SchoolVo.SchoolModel> GetCommentCollection(Guid userID, int page = 1)
        {
            var iDList = GetUserCollection(userID, 3, page);
            var list = HttpHelper.HttpPost<List<Models.Vo.SchoolVo.SchoolModel>>(
                "https://www.sxkid.com/School/GetCollectionExtAsync", Newtonsoft.Json.JsonConvert.SerializeObject(iDList));
            return list;
        }
        public List<Models.Vo.CommentVo.CommentModel> GetQACollection(Guid userID, int page = 1)
        {
            var iDList = GetUserCollection(userID, 2, page);
            var list = HttpHelper.HttpPost<List<Models.Vo.CommentVo.CommentModel>>(
                "https://www.sxkid.com/School/GetCollectionExtAsync", Newtonsoft.Json.JsonConvert.SerializeObject(iDList));
            return list;
        }
        public List<Models.Vo.QuestionVo.QuestionModel> GetArticleCollection(Guid userID, int page = 1)
        {
            var iDList = GetUserCollection(userID, 0, page);
            var list = HttpHelper.HttpPost<List<Models.Vo.QuestionVo.QuestionModel>>(
                "https://operation.sxkid.com/api/ArticleApi/GetArticles", Newtonsoft.Json.JsonConvert.SerializeObject(iDList));
            return list;
        }
    }
}
