using System;
using System.Collections.Generic;
using System.Text;
using SNSHelper_Win_Garden.Entity;
using System.Windows.Forms;

namespace SNSHelper_Win_Garden
{
    public partial class frmMain : DevComponents.DotNetBar.Office2007Form
    {
        List<Summary> summaryList = new List<Summary>();

        public void UpdateSummary(object o)
        {
            Summary summary = o as Summary;

            bool isContained = false;

            foreach (Summary item in summaryList)
            {
                if (item.UID == summary.UID)
                {
                    isContained = true;
                    item.StealTimes = summary.StealTimes;
                    item.StealedCropsNo = summary.StealedCropsNo;
                    item.WaterTimes = summary.WaterTimes;
                    item.VerminTimes = summary.VerminTimes;
                    item.GrassTimes = summary.GrassTimes;

                    foreach (DataGridViewRow dr in dgvSummary.Rows)
                    {
                        if (dr.Cells[0].Value.ToString() == string.Format("{0} ({1})", summary.Name, summary.UID))
                        {
                            dr.Cells[1].Value = summary.StealTimes;
                            dr.Cells[2].Value = summary.StealedCropsNo;
                            dr.Cells[3].Value = summary.WaterTimes;
                            dr.Cells[4].Value = summary.VerminTimes;
                            dr.Cells[5].Value = summary.GrassTimes;

                            return;
                        }
                    }
                }
            }

            if (!isContained)
            {
                summaryList.Add(summary);
            }

            dgvSummary.Rows.Add(string.Format("{0} ({1})", summary.Name, summary.UID), summary.StealTimes, summary.StealedCropsNo, summary.WaterTimes, summary.VerminTimes, summary.GrassTimes);
        }

        delegate void MethodWithObject(object o);
        MethodWithObject updateSummaryInThread;
        public void UpdateSummaryInThread(Summary summary)
        {
            this.Invoke(updateSummaryInThread, new object[] { summary });
        }

        public Summary GetSummary(string uid, string name)
        {
            foreach (Summary item in summaryList)
            {
                if (item.UID == uid)
                {
                    return item;
                }
            }

            Summary summary = new Summary(uid, name);
            summaryList.Add(summary);
            return summary;
        }
    }
}
