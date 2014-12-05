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

        public Device Device(long id, out bool isNew)
        {
            if (id < 0)
            {
                isNew = false;
                return null;
            }
            
            //fetch
            Device device = null;
            try
            {
                device = Devices.Where(d => d.ID == id).Single();
            }
            catch { }

            //create
            if (device == null)
            {
                long ts = DateTime.UtcNow.UnixTimestamp();
                Devices.InsertOnSubmit(device = new Device()
                {
                    ID = id,
                    Created = ts,
                    Updated = ts
                });
                isNew = true;
            }
            else
                isNew = false;

            return device;
        }

        public User User(long id, out bool isNew)
        {
            if (id < 0)
            {
                isNew = false;
                return null;
            }
            
            //fetch
            User user = null;
            try
            {
                user = Users.Where(d => d.ID == id).Single();
            }
            catch { }

            //create
            if (user == null)
            {
                Users.InsertOnSubmit(user = new User()
                {
                    ID = id,
                    NameFirst = "",
                    NameLast = "",
                    NameMiddle = "",
                    Type = ""
                });
                isNew = true;
            }
            else
                isNew = false;

            return user;
        }
    }
}
