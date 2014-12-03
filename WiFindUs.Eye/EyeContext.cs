using Devart.Data.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                catch (Exception)
                {
                    Debugger.E("Error submitting MySQL context changes.");
                }
            }).Start();
        }

        public Device Device(long id)
        {
            if (id < 0)
                return null;
            
            Device device = null;
            try
            {
                device = Devices.Where(d => d.ID == id).Single();
            }
            catch { }
            if (device == null)
                Devices.InsertOnSubmit(device = new Device() { ID = id });
            return device;
        }

        public User User(long id)
        {
            if (id < 0)
                return null;
            
            User user = null;
            try
            {
                user = Users.Where(d => d.ID == id).Single();
            }
            catch { }
            return user;
        }
    }
}
