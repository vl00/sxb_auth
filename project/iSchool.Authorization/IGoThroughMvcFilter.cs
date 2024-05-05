using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace iSchool.Authorization
{
    /// <summary>
    /// 用于跳过mvc filter处理
    /// </summary>
    public interface IGoThroughMvcFilter : IFilterMetadata
    {
    }
}
