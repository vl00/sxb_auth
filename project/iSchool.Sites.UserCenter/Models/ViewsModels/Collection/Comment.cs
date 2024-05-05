using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.ViewsModels.Collection
{
    public class Comment
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 学校ID
        /// </summary>
        public Guid SchoolId { get; set; }
        /// <summary>
        /// 学校学部ID
        /// </summary>
        public Guid SchoolSectionId { get; set; }
        /// <summary>
        /// 评论者ID
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 点评内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 回复数
        /// </summary>
        public int ReplyCount { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int LikeCount { get; set; }
        /// <summary>
        /// 是否匿名
        /// </summary>
        public bool IsAnony { get; set; }
        /// <summary>
        /// 是否辟谣
        /// </summary>
        public bool RumorRefuting { get; set; }
        /// <summary>
        /// 写入日期
        /// </summary>
        public string AddTime { get; set; }
        /// <summary>
        /// 是否为过来人
        /// </summary>
        public bool IsAttend { get; set; }
        /// <summary>
        /// 是否为精华
        /// </summary>
        public bool IsEssence { get; set; }
        /// <summary>
        /// 用户信息
        /// </summary>
        public dbo.UserInfo UserInfoModel { get; set; }
    }
    /// <summary>
    /// 点评回复
    /// </summary>
    public class CommentReplyModel
    {
        /// <summary>
        /// 回复id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 点评id
        /// </summary>
        public Guid SchoolCommentId { get; set; }
        /// <summary>
        /// 回复子级id
        /// </summary>
        public Guid? ReplyId { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 是否学校发布
        /// </summary>
        public bool IsSchoolPublish { get; set; }
        /// <summary>
        /// 是否就读
        /// </summary>
        public bool IsAttend { get; set; }
        /// <summary>
        /// 是否匿名
        /// </summary>
        public bool IsAnony { get; set; }
        /// <summary>
        /// 是否辟谣
        /// </summary>
        public bool RumorRefuting { get; set; }
        /// <summary>
        /// 回复内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }
        /// <summary>
        /// 回复总数
        /// </summary>
        public int ReplyCount { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int LikeCount { get; set; }
        /// <summary>
        /// 写入时间 
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
