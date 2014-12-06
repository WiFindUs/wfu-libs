using Devart.Data.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiFindUs.Extensions;

namespace WiFindUs.Eye
{
    public partial class EyeContext
    {
        public void SubmitChangesThreaded(ConflictMode conflictMode = ConflictMode.FailOnFirstConflict)
        {
            new Thread(delegate()
            {
                try
                {
                    SubmitChanges(conflictMode);
                }
                catch (Exception e)
                {
                    Debugger.Ex(e);
                }
            }).Start();
        }
    }
}
