using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class PreviousOperationCancel
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        public void PreviousHHIDDLUpdation(int machineId, string correctedDate)
        {
            //try
            //{
            //    string machineInvNo = db.tblmachinedetails.Where(x => x.IsDeleted == 0 && x.MachineID == machineId).Select(x => x.MachineInvNo).FirstOrDefault();
            //    var listPrevOp = db.tbl_PrevOperationCancel.Where(x => x.WorkCenter == machineInvNo && x.IsCancelled == 1).ToList();// && x.CorrectedDate==correctedDate for assiginig the correctedadte
            //    foreach (var row in listPrevOp)
            //    {
            //        string workOrderNo = row.ProductionOrder, operationNo = row.Operation, partNo = row.PartNumber;
            //        int qty = 0;
            //        if (row.Qty != null)
            //        {
            //            qty = (int)row.Qty;
            //        }
            //        var rowHMIDet = db.tbllivehmiscreens.Where(x => x.Work_Order_No == workOrderNo && x.OperationNo == operationNo && x.PartNo == partNo && x.MachineID == machineId && x.CorrectedDate == correctedDate && (x.Status == 2 || x.Status == 1) && (x.isWorkInProgress == 1 || x.isWorkInProgress == 0)).FirstOrDefault();
            //        if (rowHMIDet != null)
            //        {
            //            int deleveredQty = Convert.ToInt32(rowHMIDet.Delivered_Qty);
            //            int updateVal = deleveredQty - qty;
            //            rowHMIDet.Delivered_Qty = updateVal;
            //            rowHMIDet.isWorkInProgress = 0;
            //            rowHMIDet.Status = 1;
            //            db.SaveChanges();

            //            var rowDDLDet = db.tblddls.Where(x => x.IsCompleted == 1 && x.WorkOrder == workOrderNo && x.OperationNo == operationNo && x.MaterialDesc == partNo).FirstOrDefault();
            //            rowDDLDet.IsCompleted = 0;
            //            db.SaveChanges();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    IntoFile("Previous Operation Updation Error" + ex.ToString());
            //}
        }

        public bool CalPrevQty(int hmiid)
        {
            bool ret = false;
            try
            {
                //var hmiiddetails = db.tbllivehmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
                //if (hmiiddetails != null)
                //{
                //    var prevopdet = db.tbl_PrevOperationCancel.Where(m => m.ProductionOrder == hmiiddetails.Work_Order_No && m.Operation == hmiiddetails.OperationNo && m.PartNumber == hmiiddetails.PartNo && m.IsCancelled == 2).FirstOrDefault();
                //    if (prevopdet != null)
                //    {
                //        ret = true;
                //    }
                //    else
                //    {
                //        ret = false;
                //    }
                //}

            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        public bool CalPrevQtyWithWO(string woNo, string opNo, string partNo)
        {
            bool ret = false;
            try
            {
                //var prevopdet = db.tbl_PrevOperationCancel.Where(m => m.ProductionOrder == woNo && m.Operation == opNo && m.PartNumber == partNo && m.IsCancelled == 2).FirstOrDefault();
                //if (prevopdet != null)
                //{
                //    ret = true;
                //}
                //else
                //{
                //    ret = false;
                //}
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        public void IntoFile(string Msg)
        {
            try
            {
                string appPath = @"C://Tsal_log.txt";
                using (StreamWriter writer = new StreamWriter(appPath, true)) //true => Append Text
                {
                    writer.WriteLine(System.DateTime.Now + ":  " + Msg + "\r \n");
                }
            }
            catch (Exception e7)
            {
                IntoFile("IntoFile" + e7.ToString());
            }

        }
    }
}