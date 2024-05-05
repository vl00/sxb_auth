using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.Vo.CommentVo
{
    /// <summary>
    /// 点评详情
    /// </summary>
    public class CommentModel
    {
        /// <summary>
        /// 点评id
        /// </summary>
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
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 是否为过来人
        /// </summary>
        public bool IsAttend { get; set; }
        /// <summary>
        /// 是否为精华
        /// </summary>
        public bool IsEssence { get; set; }
        /// <summary>
        /// 图片数量
        /// </summary>
        public int CommentImagerCount { get; set; }
        /// <summary>
        /// 用户信息
        /// </summary>
        public UserInfoVo.UserInfoModel UserInfoModel { get; set; }
        /// <summary>
        /// 点评回复
        /// </summary>
        public CommentReplyModel CommentReplyModel { get; set; }
        /// <summary>
        /// 学校信息
        /// </summary>
        public SchoolVo.SchoolModel SchoolModel { get; set; }
    }
}
