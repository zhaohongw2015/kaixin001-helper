using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SNSHelper.Kaixin001;
using SNSHelper.Kaixin001.Entity.Parking;
using SNSHelper_Win_Parker.Entity;
using SNSHelper_Win_Parker.Helper;

namespace SNSHelper_Win_Parker
{
    public partial class frmMain
    {
        #region 账号统计
        private void parkerSta(ParkerStaInfo parkerStainfo)
        {
            Boolean bExits = false;
            foreach (DataGridViewRow dr in parkStaGridViewX1.Rows)
            {
                if (dr.Cells["UserId"].Value.ToString().Equals(parkerStainfo.UId.ToString()))
                {
                    dr.Cells["carcount"].Value = parkerStainfo.Carcount;
                    dr.Cells["CarPriceCount"].Value  = parkerStainfo.CarCountPrice;
                    dr.Cells["cash"].Value = parkerStainfo.Cashcount;
                    dr.Cells["TotalProperty"].Value = parkerStainfo.Cashcount + parkerStainfo.CarCountPrice;
                    bExits = true;
                }

            }
            if (!bExits)
            {
                parkStaGridViewX1.Rows.Add(parkerStainfo.RealName, parkerStainfo.UId, parkerStainfo.Carcount, parkerStainfo.CarCountPrice, parkerStainfo.Cashcount, (parkerStainfo.Cashcount + parkerStainfo.CarCountPrice));

            }


        }

        #endregion

    }
   
}
