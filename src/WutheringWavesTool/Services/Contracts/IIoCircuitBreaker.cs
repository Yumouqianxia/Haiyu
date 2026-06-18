using System;
using System.Collections.Generic;
using System.Text;

namespace Haiyu.Services.Contracts;
/// <summary>
/// 熔断Context操作，从主程序中进行控制
/// </summary>
public interface IIoCircuitBreaker
{
    /// <summary>
    /// 检查任务
    /// </summary>
    /// <returns></returns>
    bool TryAcquire();

    /// <summary>
    /// 释放任务
    /// </summary>
    void Release();
}
