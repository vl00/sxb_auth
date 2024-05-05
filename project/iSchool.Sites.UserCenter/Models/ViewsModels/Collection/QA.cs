using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.ViewsModels.Collection
{
    public class QA
    {
        /// <summary>
        /// 问题id
        /// </summary>
        public Guid QuestionId { get; set; }
        /// <summary>
        /// 学校分部id
        /// </summary>
        public Guid SchoolSectionId { get; set; }
        /// <summary>
        /// 问题内容
        /// </summary>
        public string QuestionContent { get; set; }
        /// <summary>
        /// 问题回答总数
        /// </summary>
        public int CurrentQuestionAnswerTotal { get; set; }
        /// <summary>
        /// 分部下总数量
        /// </summary>
        public int SectionQuestionTotal { get; set; }
        /// <summary>
        /// 问题写入时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 回答列表
        /// </summary>
        public List<AnswerModel> AnswerModels { get; set; }
    }
    /// <summary>
    /// 问答实体
    /// </summary>
    public class AnswerModel
    {
        /// <summary>
        /// 问答id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 问答详情
        /// </summary>
        public string AnswerContent { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string UserName { get; set; }
    }
}
