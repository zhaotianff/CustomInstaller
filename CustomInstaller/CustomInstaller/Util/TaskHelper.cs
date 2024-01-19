using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomInstaller.Util
{
    public class TaskHelper
    {
        public async static Task RunInThreadPool(Action action)
        {
            await Task.Run(action);
        }

        public async static Task<bool> RunInThreadPool(Func<bool> func)
        {
            return await Task.Run(func);
        }

        public async static Task RunInThreadPool(Action action,int delayMillionSeconds)
        {
            await Task.Run(action);
            await Task.Delay(delayMillionSeconds);
        }

        public async static Task<bool> RunInThreadPoolBool(Func<bool> func)
        {
            return await Task.Run(func);
        }
    }
}
