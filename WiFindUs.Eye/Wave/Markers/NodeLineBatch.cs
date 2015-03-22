using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WiFindUs.Eye.Wave.Markers
{
    public class NodeLineBatch : Drawable3D
    {
        private static readonly Regex REGEX_IPV4
            = new Regex(@"^\s*([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        [RequiredComponent]
        private NodeMarker nodeMarker;
        private Node node;
        private EyeMainForm eyeForm;
        [RequiredComponent]
        private Transform3D transform3D;
        

        protected override void Initialize()
        {
            node = nodeMarker.Entity;
            eyeForm = (WFUApplication.MainForm as EyeMainForm);
        }

        public override void Draw(TimeSpan gameTime)
        {
            foreach (DeviceMarker marker in nodeMarker.Scene.DeviceMarkers)
            {
                if (marker.Entity == null || marker.Entity.TimedOut)
                    continue;
                IPAddress addr = marker.Entity.IPAddress;
                if (addr == null)
                    continue;
                String addrString = addr.ToString();
                if (addrString.Equals("0.0.0.0"))
                    continue;
                Match match = REGEX_IPV4.Match(addrString);
                if (!match.Success)
                    continue;
                long nodeNum = -1;
                if (!Int64.TryParse(match.Groups[3].Value, out nodeNum))
                    continue;
                if (nodeNum != node.Number)
                    continue;
                RenderManager.LineBatch3D.DrawLine(transform3D.Position, marker.Transform3D.Position, Color.Blue);
            }
        }

        protected override void Dispose(bool disposing)
        {

        }
    }
}
