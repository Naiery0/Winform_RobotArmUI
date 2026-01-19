using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winform_RobotArmUI_new.Models
{
    public class Station
    {
        public string Id { get; set; }          // 이름
        public string Position { get; set; }    // 포지션
        public PointF Location { get; set; }    // 좌표
        public bool HasWafer { get; set; }      // 웨이퍼 갖고 있음?
        public Image Image { get; set; }        // 이미지

        public Station( string position, PointF loc, bool hasWafer, Image img = null)
        {
            Id = string.Empty;

            Position = position;
            Location = loc;
            HasWafer = hasWafer;
            Image = img;
        }
    }
}
