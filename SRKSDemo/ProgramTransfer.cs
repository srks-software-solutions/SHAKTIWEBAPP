using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using DNC;
//using UnitWorksMySqlConnection;
//using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using SRKSDemo.Server_Model;

namespace SRKSDemo
{
    public class ProgramTransfer
    {
        #region Variables
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        ushort port;
        String ip;
        int timeout;
        ushort h; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        ushort FlibHndl;
        string FilePath;
        #endregion

        public ProgramTransfer(String IP) //String IP
        {
            port = 8193;
            ip = IP;
            timeout = 10;
        }

        #region Methods

        //Get the Program List from the CNC Machine.
        public List<ProgramListDet> GetProgramList() //out List<ProgramListDet> PrgDetList
        {
            string retStatus = null;
            List<ProgramListDet> PrgDetList = new List<ProgramListDet>();
            Focas1.focas_ret retallclibhndl3 = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
            if (retallclibhndl3 == Focas1.focas_ret.EW_OK)
            {
                try
                {
                    int top_prog = 0;
                    short num_prog = 10;
                    Focas1.PRGDIR3 buf = new Focas1.PRGDIR3();
                    while (num_prog != 0)
                    {
                        short Retredprogdir3 = Focas1.cnc_rdprogdir3(h, 2, ref top_prog, ref num_prog, buf);
                        string Msg = null;
                        switch (Retredprogdir3)
                        {
                            case 0:
                                Msg = "Success";
                                for (int i = 1; i <= num_prog; i++)
                                {
                                    ProgramListDet PLD = new ProgramListDet();
                                    switch (i)
                                    {
                                        case 1:
                                            PLD.ProgName = buf.dir1.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir1.number.ToString();
                                            PLD.ProgSize = buf.dir1.length.ToString();
                                            PLD.ProgDate = buf.dir1.mdate.day + "-" + buf.dir1.mdate.month + "-" + buf.dir1.mdate.year + " " + buf.dir1.mdate.hour + ":" + buf.dir1.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir1: ProgramNo: " + buf.dir1.number + " Comment:" + buf.dir1.comment + " mDate:" + buf.dir1.mdate);
                                            top_prog = buf.dir1.number + 1;
                                            break;
                                        case 2:
                                            PLD.ProgName = buf.dir2.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir2.number.ToString();
                                            PLD.ProgSize = buf.dir2.length.ToString();
                                            PLD.ProgDate = buf.dir2.mdate.day + "-" + buf.dir2.mdate.month + "-" + buf.dir2.mdate.year + " " + buf.dir2.mdate.hour + ":" + buf.dir2.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir2: ProgramNo: " + buf.dir2.number + " Comment:" + buf.dir2.comment + " Page:" + buf.dir2.page);
                                            top_prog = buf.dir2.number + 1;
                                            break;
                                        case 3:
                                            PLD.ProgName = buf.dir3.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir3.number.ToString();
                                            PLD.ProgSize = buf.dir3.length.ToString();
                                            PLD.ProgDate = buf.dir3.mdate.day + "-" + buf.dir3.mdate.month + "-" + buf.dir3.mdate.year + " " + buf.dir3.mdate.hour + ":" + buf.dir3.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir3: ProgramNo: " + buf.dir3.number + " Comment:" + buf.dir3.comment + " Page:" + buf.dir3.page);
                                            top_prog = buf.dir3.number + 1;
                                            break;
                                        case 4:
                                            PLD.ProgName = buf.dir4.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir4.number.ToString();
                                            PLD.ProgSize = buf.dir4.length.ToString();
                                            PLD.ProgDate = buf.dir4.mdate.day + "-" + buf.dir4.mdate.month + "-" + buf.dir4.mdate.year + " " + buf.dir4.mdate.hour + ":" + buf.dir4.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir4: ProgramNo: " + buf.dir4.number + " Comment:" + buf.dir4.comment + " Page:" + buf.dir4.page);
                                            top_prog = buf.dir4.number + 1;
                                            break;
                                        case 5:
                                            PLD.ProgName = buf.dir5.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir5.number.ToString();
                                            PLD.ProgSize = buf.dir5.length.ToString();
                                            PLD.ProgDate = buf.dir5.mdate.day + "-" + buf.dir5.mdate.month + "-" + buf.dir5.mdate.year + " " + buf.dir5.mdate.hour + ":" + buf.dir5.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir5: ProgramNo: " + buf.dir5.number + " Comment:" + buf.dir5.comment + " mDate:" + buf.dir5.mdate);
                                            top_prog = buf.dir5.number + 1;
                                            break;
                                        case 6:
                                            PLD.ProgName = buf.dir6.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir6.number.ToString();
                                            PLD.ProgSize = buf.dir6.length.ToString();
                                            PLD.ProgDate = buf.dir6.mdate.day + "-" + buf.dir6.mdate.month + "-" + buf.dir6.mdate.year + " " + buf.dir6.mdate.hour + ":" + buf.dir6.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir6: ProgramNo: " + buf.dir6.number + " Comment:" + buf.dir6.comment + " Page:" + buf.dir6.page);
                                            top_prog = buf.dir6.number + 1;
                                            break;
                                        case 7:
                                            PLD.ProgName = buf.dir7.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir7.number.ToString();
                                            PLD.ProgSize = buf.dir7.length.ToString();
                                            PLD.ProgDate = buf.dir7.mdate.day + "-" + buf.dir7.mdate.month + "-" + buf.dir7.mdate.year + " " + buf.dir7.mdate.hour + ":" + buf.dir7.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir7: ProgramNo: " + buf.dir7.number + " Comment:" + buf.dir7.comment + " Page:" + buf.dir7.page);
                                            top_prog = buf.dir7.number + 1;
                                            break;
                                        case 8:
                                            PLD.ProgName = buf.dir8.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir8.number.ToString();
                                            PLD.ProgSize = buf.dir8.length.ToString();
                                            PLD.ProgDate = buf.dir8.mdate.day + "-" + buf.dir8.mdate.month + "-" + buf.dir8.mdate.year + " " + buf.dir8.mdate.hour + ":" + buf.dir8.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir8: ProgramNo: " + buf.dir8.number + " Comment:" + buf.dir8.comment + " Page:" + buf.dir8.page);
                                            top_prog = buf.dir8.number + 1;
                                            break;
                                        case 9:
                                            PLD.ProgName = buf.dir9.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir9.number.ToString();
                                            PLD.ProgSize = buf.dir9.length.ToString();
                                            PLD.ProgDate = buf.dir9.mdate.day + "-" + buf.dir9.mdate.month + "-" + buf.dir9.mdate.year + " " + buf.dir9.mdate.hour + ":" + buf.dir9.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir9: ProgramNo: " + buf.dir9.number + " Comment:" + buf.dir9.comment + " Page:" + buf.dir9.page);
                                            top_prog = buf.dir9.number + 1;
                                            break;
                                        case 10:
                                            PLD.ProgName = buf.dir10.comment.ToUpper();
                                            PLD.ProgNo = "O" + buf.dir10.number.ToString();
                                            PLD.ProgSize = buf.dir10.length.ToString();
                                            PLD.ProgDate = buf.dir10.mdate.day + "-" + buf.dir10.mdate.month + "-" + buf.dir10.mdate.year + " " + buf.dir10.mdate.hour + ":" + buf.dir10.mdate.minute;
                                            PrgDetList.Add(PLD);
                                            //MessageBox.Show("dir10: ProgramNo: " + buf.dir10.number + " Comment:" + buf.dir10.comment + " Page:" + buf.dir10.page);
                                            top_prog = buf.dir10.number + 1;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                break;
                            case 2:
                                Msg = "The number of readout (num_prog) is wrong." + Retredprogdir3;
                                break;
                            case 3:
                                Msg = "The start number of program (top_prog) is wrong" + Retredprogdir3;
                                break;
                            case 4:
                                Msg = "Output format (type) is wrong. " + Retredprogdir3;
                                break;
                            case 7:
                                Msg = "Write Operation is Prohibited." + Retredprogdir3;
                                break;
                            default:
                                Msg = "Error No.: " + Retredprogdir3;
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                }
                Focas1.cnc_freelibhndl(h);
            }
            else
            {
                if (retallclibhndl3 == Focas1.focas_ret.EW_SOCKET)
                {
                    //retValueInt = (int)Focas1.focas_ret.EW_SOCKET;
                    retStatus = "Socket communication error. " + retallclibhndl3.ToString();
                }
                else if (retallclibhndl3 == Focas1.focas_ret.EW_NODLL)
                {
                    //retValueInt = (int)Focas1.focas_ret.EW_NODLL;
                    retStatus = "There is no DLL file for each CNC series . " + retallclibhndl3.ToString();
                }
                else if (retallclibhndl3 == Focas1.focas_ret.EW_HANDLE)
                {
                    //retValueInt = (int)Focas1.focas_ret.EW_HANDLE;
                    retStatus = "Allocation of handle number is failed. " + retallclibhndl3.ToString();
                }

                retStatus = retallclibhndl3.ToString();
            }
            return PrgDetList;
        }

        //Get all the versions of selected CNC Program on the PC Machine Which is Stored.
        internal string GetVersionListPCProgram(String MacInv, String ProgNo, out List<ProgramVersionListDet> PrgVerList)
        {
            String RetStatus = null;
            PrgVerList = new List<ProgramVersionListDet>();
            int macId = db.tblmachinedetails.Where(m => m.MachineDisplayName == MacInv).Select(m => m.MachineID).FirstOrDefault();//Convert.ToInt32(MacInv);
            var dbItem = db.tblNcProgramTransferMains.Where(m => m.IsDeleted == false).Where(m=>m.McId== macId && m.ProgramNumber == ProgNo).OrderByDescending(m=>m.VersionNumber).ToList();
            if (dbItem.Count > 0)
            {
                    foreach (var fi in dbItem)
                    {
                        ProgramVersionListDet PD = new ProgramVersionListDet();
                        PD.ProgNo = fi.ProgramNumber;
                        PD.ProgDate = fi.CreatedDate.ToString();
                        PD.ProgVer = fi.VersionNumber.ToString();
                        PrgVerList.Add(PD);
                    }
                    RetStatus = "Success";
            }
            else
            {
                RetStatus = "No NC Programs have been saved for this Machine: " + MacInv + ".";
            }

            //String NCProgSaveFilePath = db.tbl_genericfilepath.Where(m => m.TypeofFilePath == 1 && m.IsDeleted == 0).Select(m => m.CompleteFilePath).FirstOrDefault();

            //DirectoryInfo di = new DirectoryInfo(@NCProgSaveFilePath + "\\" + MacInv);
            //if (di.Exists)
            //{
            //    FileInfo[] files = di.GetFiles(ProgNo + "_*.txt");
            //    if (files.Count() > 0)
            //    {
            //        foreach (var fi in files)
            //        {
            //            ProgramVersionListDet PD = new ProgramVersionListDet();
            //            PD.ProgNo = fi.Name.Split('_')[0].ToString();
            //            PD.ProgDate = fi.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            //            PD.ProgVer = fi.Name.Split('_')[1].Split('.')[0].ToString();
            //            PrgVerList.Add(PD);
            //        }
            //        RetStatus = "Success";
            //    }
            //    else
            //    {
            //        RetStatus = "This is a New NC Program.";
            //    }
            //}
            //else
            //{
            //    RetStatus = "No NC Programs have been saved for this Machine: " + MacInv + ".";
            //}
            return RetStatus;
        }

        //Get the Program Content from the CNC Machine - Download from CNC Machine
        internal string GetProgramDataNC(string progno, out String ProgramData) //From CNC Machine
        {
            string retStatus = null;
            ProgramData = null;
            //ProgramData = new StringBuilder();
            Focas1.focas_ret retGPL = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
            if (retGPL == Focas1.focas_ret.EW_OK)
            {
                ProgramReadError(h, 1);
                short mainprogno = Convert.ToInt16(progno.Substring(1));
                short retUploadstart = Focas1.cnc_upstart(h, mainprogno); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                Focas1.ODBUP upld = new Focas1.ODBUP();
                switch (retUploadstart)
                {
                    case 0:
                        retStatus = "Success";
                        short ret = 0;
                        ushort len = 4 + 256;
                        int LoopCount = 0;
                        //System.Threading.Thread.Sleep(3000);
                        while (ret == 0)
                        {
                            ret = Focas1.cnc_upload(h, upld, ref len);
                            int a = upld.data.Length;
                            string retValString = new string(upld.data);
                            if (ret == 10)
                            {
                                len = 256;
                                ret = 0;
                                continue;
                            }
                            else if (ret == 0)
                            {
                                ProgramData += retValString;
                            }
                            else if (ret == 2)
                            {
                                //MessageBox.Show("ret 2 :: Length " + len);
                            }
                            else if (ret == -2)
                            {

                                retStatus = "Error: " + ret;
                                ret = Focas1.cnc_upend3(h);
                                ProgramReadError(h, 0);
                                break;
                            }
                            else
                            {
                                retStatus = "Else :: Error: " + ret;
                            }

                            if (retValString.Contains('%'))
                            {
                                LoopCount++;
                                if (LoopCount >= 2)
                                {
                                    String[] Temp = ProgramData.ToString().Split('%');
                                    ProgramData = "%" + Temp[1] + "%";
                                    retStatus = ".Success";
                                    ret = Focas1.cnc_upend3(h);
                                    ProgramReadError(h, 0);
                                    break;
                                }
                            }
                            else
                            {
                                //retStatus = "Error: " + ret;
                                //MessageBox.Show("Error(cnc_upload4):" + ret);
                            }
                        }
                        ret = Focas1.cnc_upend3(h);
                        ProgramReadError(h, 0);
                        if(LoopCount == 1)
                        {
                            String[] Temp = ProgramData.ToString().Split('%');
                            ProgramData = "%" + Temp[1] + "%";
                        }
                        break;
                    case -1:
                        retStatus = "Busy";
                        break;
                    case 1:
                        retStatus = "Parameter(No.20,22:Input device) is wrong";
                        break;
                    case 7:
                        retStatus = "Write protected on CNC side";
                        break;
                    default:
                        retStatus = "cnc_upstart3 :: ErrorNo. :" + retUploadstart;
                        break;
                }
                Focas1.cnc_freelibhndl(h);
            }
            else
            {
                if (retGPL == Focas1.focas_ret.EW_SOCKET)
                {
                    //retValueInt = (int)Focas1.focas_ret.EW_SOCKET;
                    retStatus = "Socket communication error. " + retGPL.ToString();
                }
                else if (retGPL == Focas1.focas_ret.EW_NODLL)
                {
                    //retValueInt = (int)Focas1.focas_ret.EW_NODLL;
                    retStatus = "There is no DLL file for each CNC series . " + retGPL.ToString();
                }
                else if (retGPL == Focas1.focas_ret.EW_HANDLE)
                {
                    //retValueInt = (int)Focas1.focas_ret.EW_HANDLE;
                    retStatus = "Allocation of handle number is failed. " + retGPL.ToString();
                }

                retStatus = retGPL.ToString();
            }
            return retStatus;
        }

        //Get the Program Content from the PC Folder Structure
        internal string GetProgramDataPC(String MacInv, String ProgNo, int Ver, out String PCProgramData)
        {
            string retstatus = null;

            int macId = db.tblmachinedetails.Where(m => m.MachineDisplayName == MacInv).Select(m => m.MachineID).FirstOrDefault();//Convert.ToInt32(MacInv);
            var dbItem = db.tblNcProgramTransferMains.Where(m => m.IsDeleted == false).Where(m => m.McId == macId && m.ProgramNumber == ProgNo && m.VersionNumber==Ver).OrderByDescending(m => m.VersionNumber).FirstOrDefault();
            if (dbItem !=null)
            {
                PCProgramData = dbItem.ProgramData;
                
                retstatus = "Success";
            }
            else
            {
                PCProgramData = "";
                retstatus = "File is unavailable.";
            }
            return retstatus;
            //String NCProgSaveFilePath = db.tbl_genericfilepath.Where(m => m.TypeofFilePath == 1 && m.IsDeleted == 0).Select(m => m.CompleteFilePath).FirstOrDefault();
            //PCProgramData = "";
            //DirectoryInfo di = new DirectoryInfo(@NCProgSaveFilePath + "\\" + MacInv);
            ////DirectoryInfo di = new DirectoryInfo(NCProgSaveFilePath + MacInv);
            //FileInfo[] files = di.GetFiles(ProgNo + "_*.txt");
            //if (files.Count() > 0)
            //{
            //    if (Ver == 0)
            //    {
            //        // Sort by creation-time descending 
            //        Array.Sort(files, delegate (FileInfo f1, FileInfo f2)
            //        {
            //            return f2.CreationTime.CompareTo(f1.CreationTime);
            //        });

            //        //get the last file and extract its versionNo(integer after '_')
            //        string ExistingFileName = files[files.Length - 1].Name;
            //        string ExistingFileVersion = ExistingFileName.Substring(ExistingFileName.IndexOf('_') + 1, ExistingFileName.LastIndexOf('.') - (ExistingFileName.LastIndexOf('_') + 1));

            //        int ExistingFileVersionInt = 0;
            //        int.TryParse(ExistingFileVersion, out ExistingFileVersionInt);

            //        Ver = ExistingFileVersionInt;
            //    }
            //    String FilePath = NCProgSaveFilePath + "\\" + MacInv + "\\" + ProgNo + "_" + Ver + ".txt";
            //    using (StreamReader SR = new StreamReader(FilePath))
            //    {
            //        PCProgramData = SR.ReadToEnd().ToString();
            //    }
            //    retstatus = "Success";
            //}
            //else
            //{
            //    retstatus = "File is unavailable.";
            //}

        }

        //Delete the NC Program from CNC Machine
        internal string DeleteProgram(String MacInv, string programNo)
        {
            string retStatus = null;
            Focas1.focas_ret retallclibhndl3 = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
            if (retallclibhndl3 == Focas1.focas_ret.EW_OK)
            {
                short ret;
                //MessageBox.Show("Inside DeleteMethod");
                String ProgCont = null;
                //GetProgramDataNC(programNo, out ProgCont);
                //SaveNCProg(ProgCont.ToString(), MacInv, programNo.ToString(), 2);
                short mainprogno = Convert.ToInt16(programNo.Substring(1));
                ret = Focas1.cnc_delete(h, mainprogno);
                switch (ret)
                {
                    case 0:
                        retStatus = "Success";
                        break;
                    case 5:
                        retStatus = "PROGRAM " + programNo + " doesn't exist.";
                        break;
                    case 7:
                        retStatus = "Write protection on CNC side";
                        break;
                    case -1:
                        retStatus = "Data is protected.";
                        break;
                    default:
                        retStatus = "ErrorNo." + ret;
                        break;
                }
                Focas1.cnc_freelibhndl(h);
            }
            else
            {
                if (retallclibhndl3 == Focas1.focas_ret.EW_SOCKET)
                {
                    //retValueInt = (int)Focas1.focas_ret.EW_SOCKET;
                    retStatus = "Socket communication error. " + retallclibhndl3.ToString();
                }
                else if (retallclibhndl3 == Focas1.focas_ret.EW_NODLL)
                {
                    //retValueInt = (int)Focas1.focas_ret.EW_NODLL;
                    retStatus = "There is no DLL file for each CNC series . " + retallclibhndl3.ToString();
                }
                else if (retallclibhndl3 == Focas1.focas_ret.EW_HANDLE)
                {
                    //retValueInt = (int)Focas1.focas_ret.EW_HANDLE;
                    retStatus = "Allocation of handle number is failed. " + retallclibhndl3.ToString();
                }

                retStatus = retallclibhndl3.ToString();
            }
            return retStatus;
        }

        //Upload the NC Program to CNC Machine From Remote PC
        internal string UploadCNCProgram(int pthID, string FilePath1, out int retValueInt)
        {
            string retValue = null;
            //int retStatusInt = 0; //failure.
            retValueInt = 0; //EW_OK.
            try
            {
                Focas1.focas_ret retallclibhndl3 = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out FlibHndl); //library handle 
                if (retallclibhndl3 == Focas1.focas_ret.EW_OK)
                {
                    retValueInt = (int)Focas1.focas_ret.EW_OK;
                    short type = 0;
                    FilePath = FilePath1;
                    var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                    using (StreamReader sr = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        StringBuilder DataString = new StringBuilder();
                        while (!sr.EndOfStream)
                        {
                            //read the data line by line into Dictionary, then access it.
                            string line = null;
                            Dictionary<int, string> AllData = new Dictionary<int, string>();
                            int LineNo = 1;
                            while ((line = sr.ReadLine()) != null)
                            {
                                AllData.Add(LineNo, line + "\n");
                                LineNo++;
                            }
                            Focas1.focas_ret retncstart = (Focas1.focas_ret)Focas1.cnc_dwnstart3(FlibHndl, type);
                            if (retncstart == Focas1.focas_ret.EW_OK)
                            {
                                retValueInt = (int)Focas1.focas_ret.EW_OK;
                                for (int row = 1; row < AllData.Count; row++)
                                {
                                    if (DataString != null)
                                    {
                                        DataString.Clear();
                                    }
                                    if (row == 0)
                                    {
                                        DataString.Append("\n");
                                    }
                                    line = null;
                                    int stringSize = DataString.Length;
                                    ushort currentLineSize = line != null ? (ushort)line.Length : (ushort)0;
                                    while (stringSize + currentLineSize < 250) //current line + old data size < 256
                                    {
                                        if (line != null)
                                        {
                                            DataString.Append(line); //include currentline
                                        }
                                        stringSize = DataString.Length;
                                        line = null;
                                        if (AllData.ContainsKey(row))
                                        {
                                            line = AllData[row++];
                                        }
                                        else // Add "%" at the end of Program.
                                        {
                                            DataString.Append("%");
                                            break;
                                        }
                                        currentLineSize = (ushort)line.Length;
                                    }
                                    row -= 2; //Above whileLoop fails only after more data, so don't consider last row + Increment in the ForLoop so -1 => Total -2.
                                    // send cur+-rent data
                                    stringSize = DataString.Length;
                                    Object Data = DataString;
                                    {
                                        Focas1.focas_ret retcnc = (Focas1.focas_ret)Focas1.cnc_download3(FlibHndl, ref stringSize, Data);
                                        if (retcnc == Focas1.focas_ret.EW_OK)
                                        {
                                            retValue = "Executed Successfully. " + retcnc.ToString();
                                            retValueInt = (int)Focas1.focas_ret.EW_OK;
                                        }
                                        else
                                        {

                                            if (retcnc == Focas1.focas_ret.EW_FUNC)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_FUNC;
                                                retValue = "cnc_dwnstart3 function has not been executed. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_RESET)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_RESET;
                                                retValue = "Reset or stop request. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_LENGTH)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_LENGTH;
                                                retValue = "The size of character string is negative. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_DATA)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_DATA;
                                                retValue = "Data error. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_PROT)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_PROT;
                                                retValue = "Tape memory is write-protected by the CNC parameter setting. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_OVRFLOW)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_OVRFLOW;
                                                retValue = "Make enough free area in CNC memory. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_BUFFER)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_BUFFER;
                                                retValue = "Retry because the buffer is full. " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_REJECT)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_REJECT;
                                                retValue = "Downloading is disable in the current CNC status " + retcnc.ToString();
                                            }
                                            else if (retcnc == Focas1.focas_ret.EW_ALARM)
                                            {
                                                retValueInt = (int)Focas1.focas_ret.EW_ALARM;
                                                retValue = "Alarm has occurred while downloading. " + retcnc.ToString();
                                            }
                                        }
                                        retValue = retcnc.ToString();
                                    }
                                }

                                Focas1.focas_ret retncend = (Focas1.focas_ret)Focas1.cnc_dwnend3(FlibHndl);
                                if (retncend == Focas1.focas_ret.EW_OK)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_OK;
                                    retValue = "cnc_dwnend3 executed succesfully " + retncend.ToString();
                                    retValue = "Success";
                                }
                                else
                                {
                                    if (retncend == Focas1.focas_ret.EW_FUNC)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_FUNC;
                                        retValue = "cnc_dwnstart3 function has not been executed. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_DATA)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_DATA;
                                        retValue = "Data error. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_OVRFLOW)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_OVRFLOW;
                                        retValue = "Make enough free area in CNC memory. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_PROT)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_PROT;
                                        retValue = "Tape memory is write-protected by the CNC parameter setting. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_REJECT)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_REJECT;
                                        retValue = "Downloading is disable in the current CNC status. " + retncend.ToString();
                                    }
                                    else if (retncend == Focas1.focas_ret.EW_ALARM)
                                    {
                                        retValueInt = (int)Focas1.focas_ret.EW_ALARM;
                                        retValue = "Alarm has occurred while downloading. " + retncend.ToString();
                                    }
                                }
                                //retValue = retncend.ToString();
                            }
                            else
                            {
                                if (retncstart == Focas1.focas_ret.EW_BUSY)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_BUSY;
                                    retValue = "Busy. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_ATTRIB)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_ATTRIB;
                                    retValue = "Data type (type) is illegal. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_NOOPT)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_NOOPT;
                                    retValue = "No option. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_PARAM)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_PARAM;
                                    retValue = "CNC parameter error. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_MODE)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_MODE;
                                    retValue = "CNC mode error. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_REJECT)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_REJECT;
                                    retValue = "CNC is machining, so Rejected. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_ALARM)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_ALARM;
                                    retValue = "Alarm State error, reset the alarm on CNC. " + retncstart.ToString();
                                }
                                else if (retncstart == Focas1.focas_ret.EW_PASSWD)
                                {
                                    retValueInt = (int)Focas1.focas_ret.EW_PASSWD;
                                    retValue = "Specified CNC data cannot be written because the data is protected.. " + retncstart.ToString();
                                }
                            }
                            //retValue = retncstart.ToString();
                        }
                    }
                    Focas1.cnc_freelibhndl(h);
                }
                else
                {
                    if (retallclibhndl3 == Focas1.focas_ret.EW_SOCKET)
                    {
                        retValueInt = (int)Focas1.focas_ret.EW_SOCKET;
                        retValue = "Socket communication error. " + retallclibhndl3.ToString();
                    }
                    else if (retallclibhndl3 == Focas1.focas_ret.EW_NODLL)
                    {
                        retValueInt = (int)Focas1.focas_ret.EW_NODLL;
                        retValue = "There is no DLL file for each CNC series . " + retallclibhndl3.ToString();
                    }
                    else if (retallclibhndl3 == Focas1.focas_ret.EW_HANDLE)
                    {
                        retValueInt = (int)Focas1.focas_ret.EW_HANDLE;
                        retValue = "Allocation of handle number is failed. " + retallclibhndl3.ToString();
                    }

                    retValue = retallclibhndl3.ToString();
                }

                using (i_facility_shaktiEntities1 redb = new i_facility_shaktiEntities1())
                {
                    var RecordToUpdate = redb.tblprogramtransferhistories.Find(pthID);
                    if (RecordToUpdate != null)
                    {
                        RecordToUpdate.ReturnTime = DateTime.Now;
                        RecordToUpdate.ReturnStatus = retValueInt;
                        RecordToUpdate.ReturnDesc = retValue;
                        redb.Entry(RecordToUpdate).State = System.Data.Entity.EntityState.Modified;
                        redb.SaveChanges();

                    }
                    else
                    {
                        //TextLogFile("Unable to Find Latest Record to update Error/EndTime.");
                    }
                }
                Focas1.cnc_freelibhndl(FlibHndl);
            }
            catch (Exception e)
            {
                retValue += e.ToString();
            }

            return retValue;
        }

        ////Save the NC Program Being Uploaded to the CNC Machine and deleting from the CNC Machine
        //TypeOfFilePath = 1 --> Save the NC Program After Uploading to CNC Machine
        //TypeOfFilePath = 2 --> Save the NC Program Before Deleting from the CNC Machine
        internal void SaveNCProg(string Msg, string MacINV, string progNo, int TypeOfFilePath)
        {
            //create directory in name of MacInv if not exists.
            //check if file exist :: Yes: generate a new FileName.
            //write into this new file.

            String NCProgSaveFilePath = db.tbl_genericfilepath.Where(m => m.TypeofFilePath == TypeOfFilePath && m.IsDeleted == 0).Select(m => m.CompleteFilePath).FirstOrDefault();

            DirectoryInfo di = new DirectoryInfo(NCProgSaveFilePath + "\\" + MacINV);
            try
            {
                if (!di.Exists)
                {
                    di.Create();
                }

                FileInfo[] files = di.GetFiles(progNo + "_*.txt");
                if (files.Count() > 0)
                {
                    // Sort by creation-time descending 
                    Array.Sort(files, delegate (FileInfo f1, FileInfo f2)
                    {
                        return f2.CreationTime.CompareTo(f1.CreationTime);
                    });

                    //get the last file and extract its versionNo(integer after '_')
                    string ExistingFileName = files[files.Length - 1].Name;
                    string ExistingFileVersion = ExistingFileName.Substring(ExistingFileName.IndexOf('_') + 1, ExistingFileName.LastIndexOf('.') - (ExistingFileName.LastIndexOf('_') + 1));

                    int ExistingFileVersionInt = 0;
                    int.TryParse(ExistingFileVersion, out ExistingFileVersionInt);

                    int LoopVar = 0, FileCountLooperFinal = ExistingFileVersionInt;
                    while (LoopVar == 0)
                    {
                        FileInfo fi = new FileInfo(files[0].DirectoryName + "\\" + progNo + "_" + FileCountLooperFinal + ".txt");
                        if (fi.Exists)
                        {
                            FileCountLooperFinal++;
                        }
                        else
                        {
                            LoopVar++;
                            using (StreamWriter sw = fi.CreateText())
                            {
                                sw.Write(Msg);
                            }
                        }
                    }
                }
                else
                {
                    //DirectlyFileName(ProgNo_ 1.txt)

                    FileInfo fi = new FileInfo(NCProgSaveFilePath + MacINV + "\\" + progNo + "_1.txt");
                    if (fi.Exists)
                    {
                        using (StreamWriter sw = fi.CreateText())
                        {
                            sw.Write(Msg);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                //IntoFile("Error: " + e);
            }
        }

        #endregion

        internal void ProgramReadError(ushort h,int readstart)
        {
            short ReadProgPar = 3202;
            var PreadProgParMe = new Focas1.IODBPSD_1();
            var readProgParMe = new Focas1.IODBPSD_1();
            var readProgParMeBU = new Focas1.IODBPSD_1();

            int ctti = Focas1.cnc_rdparam(h, ReadProgPar, 0, 4 + 8 * Focas1.MAX_AXIS, PreadProgParMe);

            var ProgParRead = PreadProgParMe.cdata;
            readProgParMeBU = PreadProgParMe;
            if (readstart == 1)
            {
                PreadProgParMe.cdata = 64;
            }
            else if(readstart == 0)
            {
                PreadProgParMe.cdata = 24;
            }

            ctti = Focas1.cnc_wrparam(h, 4 + 1 * 1, PreadProgParMe);
        }

        public void CreateFileForProgramTransfer(string content)
        {
            try
            {
                string filepath = @"C:\NCProgram";  //Text File Path
                
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);

                }
                filepath = filepath + "\\" + "NcProgram" + ".txt";   //Text File Name
                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(content);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                e.ToString();

            }
        }

        public void DeleteFileProgramTransfer()
        {
            string filepath = @"C:\NCProgram\NcProgram.txt";
            try
            {
                if (File.Exists(filepath))
                {
                    File.Delete((filepath));
                }
            } catch (Exception e)
            {
            }
        }

    }

    public class ProgramListDet
    {
        public ProgramListDet()
        {
        }
        public ProgramListDet(string ProgName, string ProgNo, string ProgSize, string ProgDate)
        {
            this.ProgName = ProgName;
            this.ProgNo = ProgNo;
            this.ProgSize = ProgSize;
            this.ProgDate = ProgDate;
        }

        public string ProgName { set; get; }
        public string ProgNo { set; get; }
        public string ProgSize { set; get; }
        public string ProgDate { set; get; }

        public IEnumerable<ProgramListDet> PrgDetList { get; set; }
    }

    public class ProgramVersionListDet
    {
        public ProgramVersionListDet()
        {
        }
        public ProgramVersionListDet(string ProgNo, string ProgVer, string ProgDate)
        {
            this.ProgNo = ProgNo;
            this.ProgVer = ProgVer;
            this.ProgDate = ProgDate;
        }

        public string ProgNo { set; get; }
        public string ProgVer { set; get; }
        public string ProgDate { set; get; }

        public IEnumerable<ProgramVersionListDet> PrgVerList { get; set; }
    }

    #region Heidenhain NC Program Transfer



    #endregion

   

}