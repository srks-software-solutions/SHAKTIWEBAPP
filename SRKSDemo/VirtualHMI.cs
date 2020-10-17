using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using DNC;
//using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Linq;
using SRKSDemo.Server_Model;

namespace SRKSDemo
{
    #region VirtualHMI

    public class VirtualHMI:IDisposable
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        ushort port;
        String ip;
        String InventoryNo;
        int timeout;
        ushort h; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        short ret; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

        public VirtualHMI(String IP, String InvNo) //String IP
        {
            port = 8193;
            ip = IP;// "192.168.0.30";
            InventoryNo = InvNo;
            timeout = 3;

            ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        }

        //Added by Ashok for Machine Interlocking on HMI
        public VirtualHMI(String IP, out ushort  h, out short ret) //String IP
        {
            port = 8193;
            ip = IP;// "192.168.0.30";           
            timeout = 3;
            ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        }

        public void Dispose()
        { }

        //retValList : 
        internal void VirtualDispRefersh(int NoOfAxis, out List<string> retValList, out List<AxisDetails> AxisDetailsList)
        {
            var AbsPosX = new Focas1.ODBAXIS();
            var AbsPosY = new Focas1.ODBAXIS();
            var RelPosX = new Focas1.ODBAXIS();
            var RelPosY = new Focas1.ODBAXIS();
            var DTGX = new Focas1.ODBAXIS();
            var DTGY = new Focas1.ODBAXIS();
            var MacPosX = new Focas1.ODBAXIS();
            var MaxPosY = new Focas1.ODBAXIS();

            var ActFR = new Focas1.ODBACT();
            var ActSpinSpeed = new Focas1.ODBACT();

            int ctti, opti, poti, pc, cyti, progret;
            short cuttt = 6754;
            short opttt = 6752;
            short pontt = 6750;
            short cyctt = 6751;
            short partscountpar = 6712;
            var cuttim = new Focas1.IODBPSD_2(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            var opttim = new Focas1.IODBPSD_2(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            var pontim = new Focas1.IODBPSD_2(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            var cyctim = new Focas1.IODBPSD_2(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            var prodat = new Focas1.ODBUP(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            var partcount = new Focas1.IODBPSD_2();

            short progno = 9001;
            var progdata = new Focas1.ODBPRO(); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            ushort prolen = (ushort)(4 + 1024);

            Focas1.ODBDY2_2 ReadVar = new Focas1.ODBDY2_2();
            Focas1.ODBDY2_2 ReadVary = new Focas1.ODBDY2_2();
            short axisdet = 1;
            short n = 2;
            short datalength = (short)44;// (28 + 4 * (4 * n));
            short exisname = 8;

            Focas1.ODBEXAXISNAME na = new Focas1.ODBEXAXISNAME();
            Focas1.ODBAXISNAME naAx = new Focas1.ODBAXISNAME();

            short datalength1 = (short)8;// (4 + (4 * n));

            Focas1.ODBTLINF Linf = new Focas1.ODBTLINF();

            var offsetret = Focas1.cnc_rdtofsinfo(h, Linf);

            short MemTyp = Linf.ofs_type;
            short OffNo = Linf.use_no;

            short WorkOffNo = 0;

            var WorkOffRet = Focas1.cnc_rdzofsinfo(h, out WorkOffNo);

            short data_type = 1;
            short valid_fig = 0;
            short dec_fig_in = 0;
            short dec_fig_out = 0;

            var figret = Focas1.cnc_getfigure(h, data_type, out valid_fig, dec_fig_in, dec_fig_out);

            //MessageBox.Show("Valid Fig: " + valid_fig + "\nDec_Fig_In: " + dec_fig_in + "\nDec_Fig_Out: " + dec_fig_out);

            short s_no = 5;
            short e_no = 5;
            short offtype = 1;
            short offlength = (short)(8 + (4 * 1));

            Focas1.IODBTO_1_2 ToolOffset = new Focas1.IODBTO_1_2();

            short ToolOffRet = Focas1.cnc_rdtofsr(h, s_no, offtype, e_no, offlength, ToolOffset);

            //MessageBox.Show("Tool Offset Function Return value : " + ToolOffRet.ToString());

            //MessageBox.Show("First Offset Value: " + ToolOffset.ofs.m_ofs_b[0].ToString());

            short pathno = 0;
            short maxpathno = 0;

            short returnpath = Focas1.cnc_getpath(h, out pathno, out maxpathno);

            short returncal = Focas1.cnc_exaxisname(h, 0, ref exisname, na);
            short returncalAx = Focas1.cnc_rdaxisname(h, ref exisname, naAx);

            //Do this for each Axis, based on value from db for this mac.
            //short posdataret = Focas1.cnc_rddynamic2(h, axisdet, datalength, ReadVar);

            //short axisdety = 2;

            //short posdatarety = Focas1.cnc_rddynamic2(h, axisdety, datalength, ReadVary);

            //Reading Cutting Time Parameter
            ctti = Focas1.cnc_rdparam(h, cuttt, 0, 4 + 8 * Focas1.MAX_AXIS, cuttim); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            //Reading Operating Time Parameter
            opti = Focas1.cnc_rdparam(h, opttt, 0, 4 + 8 * Focas1.MAX_AXIS, opttim); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            //Reading Power On Time Parameter
            poti = Focas1.cnc_rdparam(h, pontt, 0, 4 + 8 * Focas1.MAX_AXIS, pontim); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            //Reading Cycle Time Parameter
            cyti = Focas1.cnc_rdparam(h, cyctt, 0, 4 + 8 * Focas1.MAX_AXIS, cyctim); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            //Reading the Parts Count Parameter
            pc = Focas1.cnc_rdparam(h, partscountpar, 0, 4 + 8 * Focas1.MAX_AXIS, partcount);

            Double OpTiTB = Convert.ToDouble(opttim.rdata.prm_val.ToString() + "." + opttim.rdata.dec_val.ToString());
            Double CuTiTB = Convert.ToDouble(cuttim.rdata.prm_val.ToString() + "." + cuttim.rdata.dec_val.ToString());
            Double PowOnTimeTb = Convert.ToDouble(pontim.rdata.prm_val.ToString() + "." + pontim.rdata.dec_val.ToString());
            Double CycleTimeTb = Convert.ToDouble(cyctim.rdata.prm_val.ToString() + "." + cyctim.rdata.dec_val.ToString());
            Double parttotal = Convert.ToDouble(partcount.rdata.prm_val.ToString() + "." + partcount.rdata.dec_val.ToString());

            retValList = new List<string>();

            StringBuilder sbTimeFormatop = new StringBuilder();
            StringBuilder sbTimeFormatcu = new StringBuilder();
            StringBuilder sbTimeFormatpon = new StringBuilder();

            TimeSpan op = TimeSpan.FromMinutes(OpTiTB);
            string optemp = op.ToString(@"dd\:hh\:mm\:ss");
            string[] optempArray = optemp.Split(':').Select(r => r).ToArray();
            retValList.Add("D:" + optempArray[0] + " HH:" + optempArray[1] + " MM:" + optempArray[2]); //0

            TimeSpan cu = TimeSpan.FromMinutes(CuTiTB);
            string cutemp = cu.ToString(@"dd\:hh\:mm\:ss");
            string[] cutempArray = cutemp.Split(':').Select(r => r).ToArray();
            retValList.Add("D:" + cutempArray[0] + " HH:" + cutempArray[1] + " MM:" + cutempArray[2]); //1

            TimeSpan pon = TimeSpan.FromMinutes(PowOnTimeTb);
            string pontemp = pon.ToString(@"dd\:hh\:mm\:ss");
            string[] pontempArray = pontemp.Split(':').Select(r => r).ToArray();
            retValList.Add("D:" + pontempArray[0] + " HH:" + pontempArray[1] + " MM:" + pontempArray[2]); //2

            TimeSpan cyc = TimeSpan.FromMilliseconds(CycleTimeTb);
            string cyctemp = cyc.ToString(@"dd\:hh\:mm\:ss");
            string[] cyctempArray = cyctemp.Split(':').Select(r => r).ToArray();
            retValList.Add(" HH:" + cyctempArray[1] + " MM:" + optempArray[2] + " SS:" + cyctempArray[3]); //3
            //retValList.Add(0.ToString()); //Cycle Time

            Focas1.ODBAXIS AbsAxs = new Focas1.ODBAXIS();

            var AbsAxsREt = Focas1.cnc_absolute(h, axisdet, datalength1, AbsAxs);

            StringBuilder retVal = null;
            int ReqStringLen = 6;
            AxisDetailsList = new List<AxisDetails>();
            for (short i = 1; i <= NoOfAxis; i++)
            {
                short posdataret = Focas1.cnc_rddynamic2(h, i, datalength, ReadVar);
                AxisDetails ad = new AxisDetails();
                ad.AxisID = i;
                try
                {
                    //absposxtb.Text = ReadVar.pos.absolute.ToString().Insert(3, ".");//
                    string retString = FormattedString(Convert.ToInt32(ReadVar.pos.absolute), 3, out retVal, ReqStringLen);
                    if (retString == "Success")
                    {
                        ad.AbsPos = retVal.ToString();
                    }
                    else
                    {
                        ad.AbsPos = ReadVar.pos.absolute.ToString();
                    }
                }
                catch
                {
                    //absposxtb.Text = ReadVar.pos.absolute.ToString();
                    //retValList.Add(ReadVar.pos.absolute.ToString());
                    ad.AbsPos = ReadVar.pos.absolute.ToString();
                }
                try
                {
                    //relposxtb.Text = ReadVar.pos.relative.ToString().Insert(3, ".");
                    string retString = FormattedString(Convert.ToInt32(ReadVar.pos.relative), 3, out retVal, ReqStringLen);
                    if (retString == "Success")
                    {
                        ad.RelPos = retVal.ToString();
                    }
                    else
                    {
                        ad.RelPos = ReadVar.pos.relative.ToString();
                    }
                }
                catch
                {
                    //retValList.Add(ReadVar.pos.relative.ToString());
                    ad.RelPos = ReadVar.pos.relative.ToString();
                }
                try
                {
                    //macposxtb.Text = ReadVar.pos.machine.ToString().Insert(3, ".");
                    string retString = FormattedString(Convert.ToInt32(ReadVar.pos.machine), 3, out retVal, ReqStringLen);
                    if (retString == "Success")
                    {
                        ad.MacPos = retVal.ToString();
                    }
                    else
                    {
                        ad.MacPos = ReadVar.pos.machine.ToString();
                    }
                }
                catch
                {
                    //retValList.Add(ReadVar.pos.machine.ToString());
                    ad.MacPos = ReadVar.pos.machine.ToString();
                }
                try
                {
                    //distgxtb.Text = ReadVar.pos.distance.ToString().Insert(3, ".");
                    string retString = FormattedString(Convert.ToInt32(ReadVar.pos.distance), 3, out retVal, ReqStringLen);
                    if (retString == "Success")
                    {
                        ad.DistToGo = retVal.ToString();
                    }
                    else
                    {
                        ad.DistToGo = ReadVar.pos.distance.ToString();
                    }
                }
                catch
                {
                    //retValList.Add(ReadVar.pos.distance.ToString());
                    ad.DistToGo = ReadVar.pos.distance.ToString();
                }

                AxisDetailsList.Add(ad);
            }
            #region
            //try
            //{
            //    //absposxtb.Text = ReadVar.pos.absolute.ToString().Insert(3, ".");//
            //    string retString = FormattedString(Convert.ToInt32(ReadVar.pos.absolute), 3, out retVal, ReqStringLen);//
            //    retValList.Add(retString);
            //}
            //catch
            //{
            //    //absposxtb.Text = ReadVar.pos.absolute.ToString();
            //    retValList.Add(ReadVar.pos.absolute.ToString());
            //}
            //try
            //{
            //    absposytb.Text = ReadVary.pos.absolute.ToString().Insert(3, ".");
            //}
            //catch
            //{
            //    retValList.Add(ReadVary.pos.absolute.ToString());
            //}

            //try
            //{
            //    relposxtb.Text = ReadVar.pos.relative.ToString().Insert(3, ".");
            //}
            //catch
            //{
            //    retValList.Add(ReadVar.pos.relative.ToString());
            //}
            //try
            //{
            //    relposytb.Text = ReadVary.pos.relative.ToString().Insert(3, ".");
            //}
            //catch
            //{
            //    retValList.Add(ReadVary.pos.relative.ToString());
            //}

            //try
            //{
            //    macposxtb.Text = ReadVar.pos.machine.ToString().Insert(3, ".");
            //}
            //catch
            //{
            //    retValList.Add(ReadVar.pos.machine.ToString());
            //}
            //try
            //{
            //    macposytb.Text = ReadVary.pos.machine.ToString().Insert(3, ".");
            //}
            //catch
            //{
            //    retValList.Add(ReadVary.pos.machine.ToString());
            //}
            //try
            //{
            //    distgxtb.Text = ReadVar.pos.distance.ToString().Insert(3, ".");
            //}
            //catch
            //{
            //    retValList.Add(ReadVar.pos.distance.ToString());
            //}
            //try
            //{
            //    distgytb.Text = ReadVary.pos.distance.ToString().Insert(3, ".");
            //}
            //catch
            //{
            //    distgytb.Text = ReadVary.pos.distance.ToString();
            //}
            #endregion
            Focas1.ODBACT FR = new Focas1.ODBACT();

            var ActFr = Focas1.cnc_actf(h, FR);

            short datanum = 10;
            Focas1.ODBSPLOAD SpiLoad = new Focas1.ODBSPLOAD();
            short SpiLoadRet = Focas1.cnc_rdspmeter(h, 0, ref datanum, SpiLoad);
            int SpiLoadI = SpiLoad.spload1.spload.data;
            int SpiLoadDec = SpiLoad.spload1.spload.dec;
            String SpiLoadMain = SpiLoadI.ToString().Insert(SpiLoadDec, ".");
            String SpindleLoad = SpiLoad.spload1.spload.data.ToString();

            Focas1.ODBSEQ ProgSeqNum = new Focas1.ODBSEQ();

            short SeqRet = Focas1.cnc_rdseqnum(h, ProgSeqNum);

            int BlkNo = 0;

            short BlkRet = Focas1.cnc_rdblkcount(h, out BlkNo);

            //String BlockCount = BlkNo.ToString();

            //fracttb.Text = FR.data.ToString();// ReadVar.actf.ToString();
            //spinacttb.Text = ReadVar.acts.ToString();
            //prognumtb.Text = "O" + ReadVar.prgnum.ToString() + " Sequence No: N" + ProgSeqNum.data.ToString() + " Block No: " + BlkNo; //progno.ToString();// ReadVar.prgnum.ToString();

            //FeedRate Unit
            //FWLIBAPI short WINAPI cnc_rdaxisdata(unsigned short FlibHndl, short cls, short* type, short num, short* len, ODBAXDT* axdata);
            string FeedRateUnitVal = null;
            string FeedRateUnitError = null;
            short DataLen = 64;
            Focas1.ODBAXDT FeedRateUnit = new Focas1.ODBAXDT();
            short GModalRet = Focas1.cnc_rdaxisdata(h, 5, 0, 1, ref DataLen, FeedRateUnit);
            switch (GModalRet)
            {
                case 0:
                    {
                        short unitVal = (short)FeedRateUnit.data1.unit;
                        if (unitVal != 0)
                        {
                            switch (unitVal)
                            {
                                case 3:
                                    FeedRateUnitVal = "mm/minute";
                                    break;
                                case 4:
                                    FeedRateUnitVal = "inch/minute";
                                    break;
                                case 6:
                                    FeedRateUnitVal = "mm/round";
                                    break;
                                case 7:
                                    FeedRateUnitVal = "inch/round";
                                    break;
                            }
                        }
                        break;
                    }

                case 2:
                    FeedRateUnitError = "Number of axis(*len) is less or equal 0. ";
                    break;
                case 3:
                    FeedRateUnitError = "Data class(cls) is wrong. ";
                    break;
                case 4:
                    FeedRateUnitError = "Kind of data(type) is wrong, or The number of kind(num) exceeds 4. ";
                    break;
                case 6:
                    FeedRateUnitError = "Required option to read data is not specified. ";
                    break;
            }

            retValList.Add(FR.data.ToString() + " " + FeedRateUnitVal);//FeedRate Actual 4
            retValList.Add(ReadVar.acts.ToString()); //Spindal 5
            retValList.Add("O" + ReadVar.prgnum.ToString() + " Sequence No: N" + ProgSeqNum.data.ToString() + " Block No: " + BlkNo); //Program 6

            //Reading the specified Program
            progno = (short)ReadVar.prgnum;
            //Starting the reading function
            short blknum;

            //object ProgData = "";

            Char[] ProgData = new Char[prolen];
            try
            {
                progret = Focas1.cnc_rdexecprog(h, ref prolen, out blknum, ProgData);
            }
            catch (AccessViolationException ex)
            {
                progret = Focas1.cnc_rdexecprog(h, ref prolen, out blknum, ProgData);
            }

            StringBuilder sb = new StringBuilder();
            try
            {
                String OneLine = "";

                for (int i = 0; i < prolen; i++)
                {
                    if (!OneLine.Contains("\n"))
                    {
                        OneLine += ProgData[i].ToString();
                    }
                    else
                    {
                        //sb.Append(ProgData.data[i].ToString());
                        sb.AppendLine(OneLine.Substring(0, OneLine.Length - 2));
                        OneLine = "";
                    }
                }
            }
            catch { }

            retValList.Add(sb.ToString()); //Program Execution 7

            Focas1.ODBST MacStatus = new Focas1.ODBST();

            short StatRet = Focas1.cnc_statinfo(h, MacStatus);

            retValList.Add(ip); //8
            retValList.Add(InventoryNo); //9

            //Auto/Manual Mode Status
            switch (MacStatus.aut)
            {
                case 0:
                    //statetb.Text = "MDI";
                    retValList.Add("MDI"); //10
                    break;
                case 1:
                    //statetb.Text = "MEM";
                    retValList.Add("MEM"); //10
                    break;
                case 2:
                    //statetb.Text = "****";
                    retValList.Add("****"); //10
                    break;
                case 3:
                    //statetb.Text = "EDIT";
                    retValList.Add("EDIT"); //10
                    break;
                case 4:
                    //statetb.Text = "HND";
                    retValList.Add("HND"); //10
                    break;
                case 5:
                    //statetb.Text = "JOG";
                    retValList.Add("JOG"); //10
                    break;
                case 6:
                    //statetb.Text = "Teach JOG";
                    retValList.Add("Teach JOG"); //10
                    break;
                case 7:
                    //statetb.Text = "Teach HND";
                    retValList.Add("Teach HND"); //10
                    break;
                case 8:
                    //statetb.Text = "INC Feed";
                    retValList.Add("INC Feed"); //10
                    break;
                case 9:
                    //statetb.Text = "REF";
                    retValList.Add("REF"); //10
                    break;
                case 10:
                    //statetb.Text = "RMT";
                    retValList.Add("RMT"); //10
                    break;
                default:
                    retValList.Add("****"); //10
                    break;
            }

            //Auto Run Status
            switch (MacStatus.run)
            {
                case 0:
                    //runtb.Text = "****(reset)";
                    retValList.Add("****(reset)");  //11
                    break;
                case 1:
                    //runtb.Text = "STOP";
                    retValList.Add("STOP"); //11
                    break;
                case 2:
                    //runtb.Text = "HOLD";
                    retValList.Add("HOLD"); //11
                    break;
                case 3:
                    //runtb.Text = "STRT";
                    retValList.Add("STRT"); //11
                    break;
                case 4:
                    //runtb.Text = "MSTR";
                    retValList.Add("MSTR"); //11
                    break;
                default:
                    retValList.Add("****"); //11
                    break;
            }

            //Alarm
            switch (MacStatus.alarm)
            {
                case 0:
                    //alarmtb.Text = "****";
                    retValList.Add("****"); //12
                    break;
                case 1:
                    //alarmtb.Text = "ALM";
                    //alarmtb.ForeColor = Color.DarkRed;
                    retValList.Add("ALM"); //12
                    break;
                case 2:
                    //alarmtb.Text = "BATLOW";
                    //alarmtb.ForeColor = Color.DarkRed;
                    retValList.Add("BATLOW"); //12
                    break;
                case 3:
                    //alarmtb.Text = "FANALM";
                    //alarmtb.ForeColor = Color.DarkRed;
                    retValList.Add("FANALM"); //12
                    break;
                default:
                    retValList.Add("****"); //12
                    break;
            }

            //Emergency
            switch (MacStatus.emergency)
            {
                case 0:
                    //emertb.Text = "****";
                    retValList.Add("****"); //13
                    break;
                case 1:
                    //emertb.Text = "EMG";
                    //emertb.ForeColor = Color.DarkRed;
                    retValList.Add("EMG"); //13
                    break;
                case 2:
                    //emertb.Text = "ReSET";
                    retValList.Add("ReSET"); //13
                    break;
                default:
                    retValList.Add("****"); //13
                    break;
            }

            retValList.Add(parttotal.ToString());  //14
        }

        //FOR Gentella Dashboard by Ashok
        internal void UTFValuesforMachine(out Double CycleTimeTb, out short progno, out ushort h1)
        {
            CycleTimeTb = 0;
            h1 = h;
            var cyctim = new Focas1.IODBPSD_2();
            short cyctt = 6751;
            int cyti = Focas1.cnc_rdparam(h, cyctt, 0, 4 + 8 * Focas1.MAX_AXIS, cyctim); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            CycleTimeTb = Convert.ToDouble(cyctim.rdata.prm_val.ToString() + "." + cyctim.rdata.dec_val.ToString());

            progno = 0;
            Focas1.ODBDY2_2 ReadVar = new Focas1.ODBDY2_2();
            progno = (short)ReadVar.prgnum;
        }

        internal void ModalData(ref List<string> ModelData)
        {
            //short ModalTypeG = -1;
            //short ModalBlockG = 0;
            short ModalTypeT = 108;
            short ModalBlockT1 = 0;
            short ModalBlockT2 = 1;
            short ModalBlockT3 = 2;

            Focas1.ODBMDL_2 ModaldataG = new Focas1.ODBMDL_2();
            Focas1.ODBMDL_3 ModaldataT1 = new Focas1.ODBMDL_3();
            Focas1.ODBMDL_3 ModaldataT2 = new Focas1.ODBMDL_3();
            Focas1.ODBMDL_3 ModaldataT3 = new Focas1.ODBMDL_3();

            //short GModalRet = Focas1.cnc_modal(h, ModalTypeG, ModalBlockG, ModaldataG);
            short TModalRet1 = Focas1.cnc_modal(h, ModalTypeT, ModalBlockT1, ModaldataT1);
            short TModalRet2 = Focas1.cnc_modal(h, ModalTypeT, ModalBlockT2, ModaldataT2);
            short TModalRet3 = Focas1.cnc_modal(h, ModalTypeT, ModalBlockT3, ModaldataT3);

            //hdtoollbl.Text = ModaldataT1.aux.aux_data.ToString();
            //nxtoollbl.Text = ModaldataT2.aux.aux_data.ToString();
            //NxTl1lbl.Text = ModaldataT3.aux.aux_data.ToString();

            //ModelData = new List<string>();

            //ModelData.Clear();
            ModelData.Add(ModaldataT1.aux.aux_data.ToString());
            ModelData.Add(ModaldataT2.aux.aux_data.ToString());
            ModelData.Add(ModaldataT3.aux.aux_data.ToString());

            //g1lbl.Text = ModaldataG.g_rdata[0].ToString();
            //g2lbl.Text = ModaldataG.g_rdata[1].ToString();

            // 21 Data only
            for (int i = 0; i < 21; i++)
            {
                ModelData.Add(GetGModalData((short)i));
            }
        }

        private string GetGModalData(short ModalTypeG)
        {
            short ModalBlockG = 0;
            Focas1.ODBMDL_1 ModaldataG = new Focas1.ODBMDL_1();
            short GModalRet = Focas1.cnc_modal(h, ModalTypeG, ModalBlockG, ModaldataG);

            return ModaldataG.g_data.ToString();
        }

        private void ServoDet(ushort h)
        {
            short DataLen = 3;
            Focas1.ODBAXDT ServLoad = new Focas1.ODBAXDT();
            Focas1.ODBAXDT ServCurrPer = new Focas1.ODBAXDT();
            Focas1.ODBAXDT ServCurrAmp = new Focas1.ODBAXDT();
            var ServRetLoad = Focas1.cnc_rdaxisdata(h, 2, 0, 1, ref DataLen, ServLoad);
            var ServRetCurPer = Focas1.cnc_rdaxisdata(h, 2, 1, 1, ref DataLen, ServCurrPer);
            var ServRetCurAmp = Focas1.cnc_rdaxisdata(h, 2, 2, 1, ref DataLen, ServCurrAmp);

            String AxisName = ServLoad.data1.name;
            int SpiLoadI1 = ServLoad.data1.data;
            int SpiLoadDec1 = ServLoad.data1.dec;
            String ServLoadMain = SpiLoadI1.ToString();
            if (SpiLoadDec1 != 0)
            {
                ServLoadMain = SpiLoadI1.ToString().Insert(SpiLoadDec1, ".");
            }
            int ServcurrPerI = ServCurrPer.data1.data;
            int ServcurrPerD = ServCurrPer.data1.dec;
            String ServCurPerMain = ServcurrPerI.ToString();
            if (Math.Abs(ServcurrPerI).ToString().Length > 2 && ServcurrPerD > 0)
            {
                ServCurPerMain = ServcurrPerI.ToString().Insert(ServcurrPerD, ".");
            }
            int ServcurrAmpI = ServCurrAmp.data1.data;
            int ServcurrAmpD = ServCurrAmp.data1.dec;
            String ServcurrAmpMain = ServcurrAmpI.ToString();
            if (Math.Abs(ServcurrAmpI).ToString().Length > 2 && ServcurrAmpD > 0)
            {
                ServcurrAmpMain = ServcurrAmpI.ToString().Insert(ServcurrAmpD, ".");
            }

            String AxisName2 = ServLoad.data2.name;
            int SpiLoadI2 = ServLoad.data2.data;
            int SpiLoadDec2 = ServLoad.data2.dec;
            String ServLoadMain2 = SpiLoadI2.ToString();
            if (SpiLoadDec2 != 0)
            {
                ServLoadMain2 = SpiLoadI2.ToString().Insert(SpiLoadDec2, ".");
            }
            int ServcurrPerI2 = ServCurrPer.data2.data;
            int ServcurrPerD2 = ServCurrPer.data2.dec;
            String ServCurPerMain2 = ServcurrPerI2.ToString();
            if (Math.Abs(ServcurrPerI2).ToString().Length > 2 && ServcurrPerD2 > 0)
            {
                ServCurPerMain2 = ServcurrPerI2.ToString().Insert(ServcurrPerD2, ".");
            }
            int ServcurrAmpI2 = ServCurrAmp.data2.data;
            int ServcurrAmpD2 = ServCurrAmp.data2.dec;
            String ServcurrAmpMain2 = ServcurrAmpI2.ToString();
            if (Math.Abs(ServcurrAmpI2).ToString().Length > 2 && ServcurrAmpD2 > 0)
            {
                ServcurrAmpMain2 = ServcurrAmpI2.ToString().Insert(ServcurrAmpD2, ".");
            }

            String AxisName3 = ServLoad.data3.name;
            int SpiLoadI3 = ServLoad.data3.data;
            int SpiLoadDec3 = ServLoad.data3.dec;
            String ServLoadMain3 = SpiLoadI3.ToString();
            if (SpiLoadDec3 != 0)
            {
                ServLoadMain3 = SpiLoadI3.ToString().Insert(SpiLoadDec3, ".");
            }
            int ServcurrPerI3 = ServCurrPer.data3.data;
            int ServcurrPerD3 = ServCurrPer.data3.dec;
            String ServCurPerMain3 = ServcurrPerI3.ToString();
            if (Math.Abs(ServcurrPerI3).ToString().Length > 2 && ServcurrPerD3 > 0)
            {
                ServCurPerMain3 = ServcurrPerI3.ToString().Insert(ServcurrPerD3, ".");
            }
            int ServcurrAmpI3 = ServCurrAmp.data3.data;
            int ServcurrAmpD3 = ServCurrAmp.data3.dec;
            String ServcurrAmpMain3 = ServcurrAmpI3.ToString();
            if (Math.Abs(ServcurrAmpI3).ToString().Length > 2 && ServcurrAmpD3 > 0)
            {
                ServcurrAmpMain3 = ServcurrAmpI3.ToString().Insert(ServcurrAmpD3, ".");
            }

            String AxisName4 = ServLoad.data4.name;
            int SpiLoadI4 = ServLoad.data4.data;
            int SpiLoadDec4 = ServLoad.data4.dec;
            String ServLoadMain4 = SpiLoadI4.ToString();
            if (SpiLoadDec4 != 0)
            {
                ServLoadMain4 = SpiLoadI4.ToString().Insert(SpiLoadDec4, ".");
            }
            int ServcurrPerI4 = ServCurrPer.data4.data;
            int ServcurrPerD4 = ServCurrPer.data4.dec;
            String ServCurPerMain4 = ServcurrPerI4.ToString();
            if (Math.Abs(ServcurrPerI4).ToString().Length > 2 && ServcurrPerD4 > 0)
            {
                ServCurPerMain4 = ServcurrPerI4.ToString().Insert(ServcurrPerD4, ".");
            }
            int ServcurrAmpI4 = ServCurrAmp.data4.data;
            int ServcurrAmpD4 = ServCurrAmp.data4.dec;
            String ServcurrAmpMain4 = ServcurrAmpI4.ToString();
            if (Math.Abs(ServcurrAmpI4).ToString().Length > 2 && ServcurrAmpD4 > 0)
            {
                ServcurrAmpMain4 = ServcurrAmpI4.ToString().Insert(ServcurrAmpD4, ".");
            }

        }

        private void TVSParameterCheck(ushort h)
        {
            var rdpmcdataMAP = new Focas1.IODBPMC0();
            var rdpmcdataPCP = new Focas1.IODBPMC0();
            var rdpmcdataPSP = new Focas1.IODBPMC0();
            var rdpmcdataLOL = new Focas1.IODBPMC0();
            var rdpmcdataCOL = new Focas1.IODBPMC0();
            var rdpmcdataLOP = new Focas1.IODBPMC0();
            var rdpmcdataLOP1 = new Focas1.IODBPMC0();
            short adr_type = 9;
            short data_type = 0;
            ushort s_numberMAP = (ushort)8888;
            ushort e_numberMAP = (ushort)8888;
            ushort s_numberPCP = (ushort)8889;
            ushort e_numberPCP = (ushort)8889;
            ushort s_numberPSP = (ushort)8890;
            ushort e_numberPSP = (ushort)8890;
            ushort s_numberLOL = (ushort)8892;
            ushort e_numberLOL = (ushort)8892;
            ushort s_numberCOL = (ushort)8893;
            ushort e_numberCOL = (ushort)8893;
            ushort s_numberLOP = (ushort)8891;
            ushort e_numberLOP = (ushort)8891;
            ushort s_numberLOP1 = (ushort)8894;
            ushort e_numberLOP1 = (ushort)8894;
            ushort length = 9;
            short rdret = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberMAP, e_numberMAP, length, rdpmcdataMAP);
            short rdret1 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberPCP, e_numberPCP, length, rdpmcdataPCP);
            short rdret2 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberPSP, e_numberPSP, length, rdpmcdataPSP);
            short rdret3 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberLOL, e_numberLOL, length, rdpmcdataLOL);
            short rdret4 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberCOL, e_numberCOL, length, rdpmcdataCOL);
            short rdret5 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberLOP, e_numberLOP, length, rdpmcdataLOP);
            short rdret6 = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_numberLOP1, e_numberLOP1, length, rdpmcdataLOP1);

            if (Convert.ToInt32(rdpmcdataMAP.cdata[0]) == 1)
            {

            }

            if (Convert.ToInt32(rdpmcdataPCP.cdata[0]) == 1)
            {

            }

            if (Convert.ToInt32(rdpmcdataPSP.cdata[0]) == 1)
            {

            }

            if (Convert.ToInt32(rdpmcdataLOL.cdata[0]) == 1)
            {

            }

            if (Convert.ToInt32(rdpmcdataCOL.cdata[0]) == 1)
            {

            }

            if (Convert.ToInt32(rdpmcdataLOP.cdata[0]) == 1 && Convert.ToInt32(rdpmcdataLOP1.cdata[0]) == 1)
            {

            }


        }

        public static string FormattedString(int IntVal, int DecimalPos, out StringBuilder retVal, int ReqStringLen)
        {
            string retStatus = null;
            if (DecimalPos <= IntVal.ToString().PadLeft(ReqStringLen, '0').Length)
            {
                string paddedStringVal = IntVal.ToString().Replace('-', ' ').Trim().PadLeft(ReqStringLen, '0').Insert(DecimalPos, ".");
                //Console.WriteLine(""+ paddedStringVal);
                retVal = new StringBuilder(string.Format("{0:000.000}", paddedStringVal));
                if (IntVal < 0)
                {
                    retVal.Insert(0, "-");
                }
                return retStatus = "Success";
            }
            else
            {
                retVal = new StringBuilder(IntVal.ToString());
                return retStatus = "InvalidDecimalPosition";
            }
        }

        //public bool IncrementATCCounter(int MacID, int CurATCVal)
        //{
        //    DataTable dataHolder = new DataTable();
        //    using (MsqlConnection mc = new MsqlConnection())
        //    {
        //        mc.open();
        //        string SelectQuery = "SELECT ATCValue,Counter FROM gtkdaq.tblatccounter where MachineID = " + MacID + " limit 1 ;";
        //        MyMySqlDataAdapter da1 = new MyMySqlDataAdapter(SelectQuery, mc.msqlConnection);
        //        da1.Fill(dataHolder);
        //        mc.close();
        //    }
        //    if (dataHolder.Rows.Count > 0)
        //    {
        //        int prvATCVal = Convert.ToInt32(dataHolder.Rows[0][0]);
        //        if (Convert.ToInt32(prvATCVal) < CurATCVal)
        //        {
        //            int CounterVal = Convert.ToInt32(dataHolder.Rows[0][1]);
        //            CounterVal++;
        //            MsqlConnection mc = new MsqlConnection();
        //            mc.open();
        //            MySqlCommand cmdUpdateRows = new MySqlCommand("update gtkdaq.tblatccounter set ATCValue = " + CurATCVal + " and Counter = " + CounterVal + " where MachineID = " + MacID + ";");
        //            cmdUpdateRows.ExecuteNonQuery();
        //            mc.close();
        //        }
        //    }
        //    else
        //    {
        //        MsqlConnection mc = new MsqlConnection();
        //        mc.open();
        //        MySqlCommand cmdUpdateRows = new MySqlCommand("Insert into gtkdaq.tblatccounter (MachineID,Counter,InsertedOn,ATCValue) values (" + MacID + ",0,'" + DateTime.Now + "',0)");
        //        cmdUpdateRows.ExecuteNonQuery();
        //        mc.close();
        //    }
        //    return false;
        //}

    }

    public class AxisDetails
    {
        public AxisDetails()
        {
        }
        public AxisDetails(string AbsPos, int AxisID, string AxisName, string RelPos, string MacPos, string DistToGo)
        {
            this.AbsPos = AbsPos;
            this.AxisID = AxisID;
            this.AxisName = AxisName;
            this.DistToGo = DistToGo;
            this.MacPos = MacPos;
            this.RelPos = RelPos;
        }

        //public int AxisID { set { AxisID = value; } get { return AxisID; } }
        //public string AxisName { set { AxisName = value; } get { return AxisName; } }
        //public string AbsPos { set { AbsPos = value; } get { return AbsPos; } }
        //public string RelPos { set { RelPos = value; } get { return RelPos; } }
        //public string MacPos { set { MacPos = value; } get { return MacPos; } }
        //public string DistToGo { set { DistToGo = value; } get { return DistToGo; } }

        public int AxisID { set; get; }
        public string AxisName { set; get; }
        public string AbsPos { set; get; }
        public string RelPos { set; get; }
        public string MacPos { set; get; }
        public string DistToGo { set; get; }
    }

    #endregion

    #region VirtualHMI Alarm
    public class AlarmDetails
    {
        public AlarmDetails()
        {
        }
        public AlarmDetails(string AlarmNo, string AxisName, string AlarmDesc, string OccurredOn)
        {
            this.AlarmNo = AlarmNo;
            this.AxisName = AxisName;
            this.AlarmDesc = AlarmDesc;
            this.OccurredOn = OccurredOn;
        }

        public string AxisName { set; get; }
        public string AlarmNo { set; get; }
        public string AlarmDesc { set; get; }
        public string OccurredOn { set; get; }

        public IEnumerable<AlarmDetails> AlarmList { get; set; }
    }

    public class AlarmHMI
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        ushort port;
        String ip;
        String InventoryNo;
        int timeout;
        ushort h; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        short ret; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

        public AlarmHMI(String IP, String InvNo, int Type) //String IP
        {
            //InitializeComponent();
            port = 8193;
            ip = IP;// "192.168.0.30";
            InventoryNo = InvNo;
            timeout = 10;

            ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            //Focas1.ODBSYS MacInfo = new Focas1.ODBSYS();
            //short SysInfoRet = Focas1.cnc_sysinfo(h, MacInfo);
        }

        //internal void GetAlarmHistory(out List<AlarmDetails> AlarmDetailsList)
        //{
        //    AlarmDetailsList = new List<AlarmDetails>();

        //    int alarmmsgret;
        //    ushort msglen = (ushort)(4 + (516 * 10));
        //    int opmsgret;

        //    var Almmsg = new Focas1.ODBAHIS5();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
        //    var Almmsg1 = new Focas1.ODBAHIS5();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
        //    var Almmsg2 = new Focas1.ODBAHIS5();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
        //    var Almmsg3 = new Focas1.ODBAHIS5();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
        //    var Almmsg4 = new Focas1.ODBAHIS5();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM

        //    //Stopping Operation History While logging data from Message History and Alarm History
        //    alarmmsgret = Focas1.cnc_stopophis(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

        //    ////Logging Alarm History
        //    opmsgret = Focas1.cnc_rdalmhistry5(h, 1, 10, msglen, Almmsg); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
        //    opmsgret = Focas1.cnc_rdalmhistry5(h, 11, 20, msglen, Almmsg1); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
        //    opmsgret = Focas1.cnc_rdalmhistry5(h, 21, 30, msglen, Almmsg2); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
        //    opmsgret = Focas1.cnc_rdalmhistry5(h, 31, 40, msglen, Almmsg3); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
        //    opmsgret = Focas1.cnc_rdalmhistry5(h, 41, 50, msglen, Almmsg4); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM

        //    //Starting Operation History Logging
        //    opmsgret = Focas1.cnc_startophis(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

        //    if (opmsgret == 0)
        //    {
        //        string alamsgdate1 = null, alamsgtime1 = null, alamsgdatetime1 = null, alamsgalmno1 = null, alamsgaxisno1 = null, alamsgaxisnum1 = null, alamsgmsg1 = null, alamsgabspos1 = null;
        //        String alamgrp1 = null;
        //        //saving alarm msg history
        //        //1st message
        //        if (Almmsg.alm_his.data1.day.ToString() != "0")
        //        {
        //            alamsgdate1 = Almmsg.alm_his.data1.year.ToString() + "/" + Almmsg.alm_his.data1.month.ToString() + "/" + Almmsg.alm_his.data1.day.ToString();
        //            String datesssss = Almmsg.alm_his.data1.year.ToString() + "-" + Almmsg.alm_his.data1.month.ToString() + "-" + Almmsg.alm_his.data1.day.ToString();

        //            alamsgtime1 = Almmsg.alm_his.data1.hour.ToString() + ":" + Almmsg.alm_his.data1.minute.ToString() + ":" + Almmsg.alm_his.data1.second.ToString();
        //            alamsgdatetime1 = datesssss + " " + alamsgtime1;
        //            alamsgalmno1 = Almmsg.alm_his.data1.alm_no.ToString();
        //            alamsgaxisno1 = Almmsg.alm_his.data1.axis_no.ToString();
        //            //alamsgaxisnum1 = opmsg.alm_his.data1..ToString();
        //            alamsgmsg1 = Almmsg.alm_his.data1.alm_msg.ToString();
        //            //alamsgabspos1 = opmsg.alm_his.data1.abs_pos.ToString();
        //            alamgrp1 = Almmsg.alm_his.data1.alm_grp.ToString();
        //            //System.Windows.Forms.MessageBox.Show(machineid + "\n" + alamgrp1 + "\n" + alamsgalmno1 + "\n" + alamsgmsg1);
        //        }

        //    }
        //}

        //Alarm History Type 1
        internal void formatinsertAlmHis(out List<AlarmDetails> AlarmDetailsList)
        {
            var opmsg = new Focas1.ODBAHIS5();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
            ushort msgnumbers;

            AlarmDetailsList = new List<AlarmDetails>();

            //Stopping Operation History While logging data from Message History and Alarm History
            int alarmmsgret = Focas1.cnc_stopophis(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            int msgnumret = Focas1.cnc_rdalmhisno(h, out msgnumbers);
            if (msgnumbers != 0)
            {
                int snoval = 1;

                ushort msglen = (ushort)(4 + (516 * 10));
                ushort s_no = (ushort)(msgnumbers - snoval);
                ushort e_no = msgnumbers;
                for (int i = 0; i < msgnumbers; i++)
                {
                    AlarmDetails AD = new AlarmDetails();
                    int opmsgret1 = Focas1.cnc_rdalmhistry5(h, s_no, e_no, msglen, opmsg); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM

                    if (opmsgret1 == 0)
                    {
                        string alamsgdate1 = null, alamsgtime1 = null, alamsgdatetime1 = null, alamsgalmno1 = null, alamsgaxisno1 = null, alamsgaxisnum1 = null, alamsgmsg1 = null, alamsgabspos1 = null;
                        String alamgrp1 = null;
                        //saving alarm msg history
                        //1st message
                        if (opmsg.alm_his.data1.day.ToString() != "0")
                        {
                            alamsgdate1 = opmsg.alm_his.data1.year.ToString() + "/" + opmsg.alm_his.data1.month.ToString() + "/" + opmsg.alm_his.data1.day.ToString();
                            String datesssss = opmsg.alm_his.data1.year.ToString() + "-" + opmsg.alm_his.data1.month.ToString() + "-" + opmsg.alm_his.data1.day.ToString();
                            alamsgtime1 = opmsg.alm_his.data1.hour.ToString() + ":" + opmsg.alm_his.data1.minute.ToString() + ":" + opmsg.alm_his.data1.second.ToString();
                            alamsgdatetime1 = datesssss + " " + alamsgtime1;
                            alamsgalmno1 = opmsg.alm_his.data1.alm_no.ToString();
                            alamsgaxisno1 = opmsg.alm_his.data1.axis_no.ToString();
                            alamsgaxisnum1 = opmsg.alm_his.data1.axis_num.ToString();
                            alamsgmsg1 = opmsg.alm_his.data1.alm_msg.ToString();
                            alamsgabspos1 = opmsg.alm_his.data1.abs_pos.ToString();
                            alamgrp1 = opmsg.alm_his.data1.alm_grp.ToString();
                        }

                        //1st Msg
                        if (alamsgdate1 != null)
                        {
                            String alarmmsgnumber = alamsgalmno1;
                            if (alamgrp1 == "6")
                            {
                                alarmmsgnumber = "SV" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "9")
                            {
                                alarmmsgnumber = "SP" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "0")
                            {
                                alarmmsgnumber = "SW" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "1")
                            {
                                alarmmsgnumber = "PW" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "5")
                            {
                                alarmmsgnumber = "OH" + alamsgalmno1;
                            }

                            AD.AlarmDesc = alamsgmsg1.ToUpper();
                            AD.AlarmNo = alarmmsgnumber;
                            AD.AxisName = alamsgaxisno1;
                            AD.OccurredOn = Convert.ToDateTime(alamsgdatetime1).ToString("dd-MM-yyyy HH:mm:ss");
                            AlarmDetailsList.Add(AD);
                        }
                    }

                    e_no = (ushort)(e_no - 1);
                    s_no = (ushort)(e_no - 1);
                    if (s_no == 0)
                    {
                        s_no = 1;
                    }
                }
                //Starting Operation History Logging
                int opmsgret = Focas1.cnc_startophis(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            }
            var stopret = Focas1.cnc_startophis(h);
        }

        //Alarm History Type 2
        internal void formatinsertAlmHis2(out List<AlarmDetails> AlarmDetailsList)
        {
            var opmsg = new Focas1.ODBAHIS();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
            ushort msgnumbers;
            var stopret = Focas1.cnc_stopophis(h);
            int msgnumret = Focas1.cnc_rdalmhisno(h, out msgnumbers);
            AlarmDetailsList = new List<AlarmDetails>();
            if (msgnumbers != 0)
            {
                int snoval = 1;

                ushort msglen = (ushort)(4 + (516 * 10));
                ushort s_no = (ushort)(msgnumbers - snoval);
                ushort e_no = msgnumbers;
                //Stopping Operation History While logging data from Message History and Alarm History
                int alarmmsgret = Focas1.cnc_stopophis(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
                for (int i = 0; i < msgnumbers; i++)
                {
                    AlarmDetails AD = new AlarmDetails();
                    int opmsgret1 = Focas1.cnc_rdalmhistry(h, e_no, e_no, msglen, opmsg); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                    if (opmsgret1 == 0)
                    {
                        string alamsgdate1 = null, alamsgtime1 = null, alamsgdatetime1 = null, alamsgalmno1 = null, alamsgaxisno1 = null, alamsgaxisnum1 = null, alamsgmsg1 = null, alamsgabspos1 = null;
                        String alamgrp1 = null;
                        //saving alarm msg history
                        //1st message
                        if (opmsg.alm_his.data1.day.ToString() != "0")
                        {
                            alamsgdate1 = opmsg.alm_his.data1.year.ToString() + "/" + opmsg.alm_his.data1.month.ToString() + "/" + opmsg.alm_his.data1.day.ToString();
                            String datesssss = opmsg.alm_his.data1.year.ToString() + "-" + opmsg.alm_his.data1.month.ToString() + "-" + opmsg.alm_his.data1.day.ToString();

                            alamsgtime1 = opmsg.alm_his.data1.hour.ToString() + ":" + opmsg.alm_his.data1.minute.ToString() + ":" + opmsg.alm_his.data1.second.ToString();
                            alamsgdatetime1 = datesssss + " " + alamsgtime1;
                            alamsgalmno1 = opmsg.alm_his.data1.alm_no.ToString();
                            alamsgaxisno1 = opmsg.alm_his.data1.axis_no.ToString();
                            //alamsgaxisnum1 = opmsg.alm_his.data1..ToString();
                            alamsgmsg1 = opmsg.alm_his.data1.alm_msg.ToString();
                            //alamsgabspos1 = opmsg.alm_his.data1.abs_pos.ToString();
                            alamgrp1 = opmsg.alm_his.data1.alm_grp.ToString();
                            //System.Windows.Forms.MessageBox.Show(machineid + "\n" + alamgrp1 + "\n" + alamsgalmno1 + "\n" + alamsgmsg1);
                        }

                        //1st Msg
                        if (alamsgdate1 != null)
                        {
                            String alarmmsgnumber = alamsgalmno1;
                            if (alamgrp1 == "6")
                            {
                                alarmmsgnumber = "SV" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "9")
                            {
                                alarmmsgnumber = "SP" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "0")
                            {
                                alarmmsgnumber = "SW" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "1")
                            {
                                alarmmsgnumber = "PW" + alamsgalmno1;
                            }
                            else if (alamgrp1 == "5")
                            {
                                alarmmsgnumber = "OH" + alamsgalmno1;
                            }

                            //MySqlCommand cmd1 = new MySqlCommand("INSERT INTO alarm_history_master(AlarmMessage,AlarmDate,AlarmTime,AlarmDateTime,AlarmNo,Axis_No,Axis_Num,Abs_Pos,InsertedOn,MachineID,Shift,CorrectedDate)" +
                            //    "VALUES('" + alamsgmsg1 + "','" + alamsgdate1 + "','" + alamsgtime1 + "','" + alamsgdatetime1 + "','" + alarmmsgnumber + "','" + alamsgaxisno1 + "','" + alamsgaxisnum1 + "','" + alamsgabspos1 + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + machineid + ",'" + shift + "','" + CorrectedDate + "')", mc.msqlConnection);
                            //cmd1.ExecuteNonQuery();
                            AD.AlarmDesc = alamsgmsg1.ToUpper();
                            AD.AlarmNo = alarmmsgnumber;
                            AD.AxisName = alamsgaxisno1;
                            AD.OccurredOn = Convert.ToDateTime(alamsgdatetime1).ToString("dd-MM-yyyy HH:mm:ss");
                            AlarmDetailsList.Add(AD);
                        }
                    }
                    e_no = (ushort)(e_no - 1);
                    s_no = (ushort)(e_no - 1);
                    if (e_no == 0)
                    {
                        e_no = 0;
                        s_no = 0;
                    }
                }
                //Starting Operation History Logging
                int opmsgret = Focas1.cnc_startophis(h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
            }
            stopret = Focas1.cnc_startophis(h);
        }
    }
    #endregion

    #region VirtualHMI Operator Message History

    public class OpMsgHisDetails
    {
        public OpMsgHisDetails()
        {
        }
        public OpMsgHisDetails(string MsgNo, string MsgDesc, string OccurredOn)
        {
            this.MsgNo = MsgNo;
            this.MsgDesc = MsgDesc;
            this.OccurredOn = OccurredOn;
        }

        public string MsgNo { set; get; }
        public string MsgDesc { set; get; }
        public string OccurredOn { set; get; }

        public IEnumerable<OpMsgHisDetails> OpMsgList { get; set; }
    }

    public class OpMsgHMI
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        ushort port;
        String ip;
        String InventoryNo;
        int timeout;
        ushort h; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        short ret; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

        public OpMsgHMI(String IP, String InvNo, int Type) //String IP
        {
            //InitializeComponent();
            port = 8193;
            ip = IP;// "192.168.0.30";
            InventoryNo = InvNo;
            timeout = 10;
            ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        }

        //Operator Message History Type 1
        internal void formatinsertMsgHis(out List<OpMsgHisDetails> OpMsgDetailsList)
        {
            var alarmmsg1 = new Focas1.ODBOMHIS2();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
            //var alarmmsg2 = new Focas1.ODBOMHIS2();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
            ushort msgnumbers;
            var stopret = Focas1.cnc_stopophis(h);
            int msgnumret = Focas1.cnc_rdomhisno(h, out msgnumbers);
            OpMsgDetailsList = new List<OpMsgHisDetails>();
            if (msgnumbers != 0)
            {
                int snoval = 1;
                ushort msglent = (ushort)(4 + alarmmsg1.opm_his.data1.alm_msg.Length * 2); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                ushort s_no = (ushort)(msgnumbers - snoval);
                ushort e_no = msgnumbers;
                for (int i = 0; i < msgnumbers; i++)
                {
                    OpMsgHisDetails OD = new OpMsgHisDetails();
                    int alarmmsgret = Focas1.cnc_rdomhistry2(h, e_no, e_no, msglent, alarmmsg1); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                    if (alarmmsgret == 0)
                    {
                        String opmsgdate1 = null, opmsgtime1 = null, opmsgdatetime1 = null, opmsgmsg1 = null, opmsgno1 = null, opmsgcode1 = null;
                        // saving date from operator msg history
                        if (alarmmsg1.opm_his.data1.day.ToString() != "0")
                        {
                            opmsgdate1 = alarmmsg1.opm_his.data1.year.ToString() + "/" + alarmmsg1.opm_his.data1.month.ToString() + "/" + alarmmsg1.opm_his.data1.day.ToString();
                            opmsgtime1 = alarmmsg1.opm_his.data1.hour.ToString() + ":" + alarmmsg1.opm_his.data1.minute.ToString() + ":" + alarmmsg1.opm_his.data1.second.ToString();
                            String Datessss = alarmmsg1.opm_his.data1.year.ToString() + "-" + alarmmsg1.opm_his.data1.month.ToString() + "-" + alarmmsg1.opm_his.data1.day.ToString();
                            opmsgdatetime1 = Convert.ToDateTime(Datessss).ToString("yyyy-MM-dd") + " " + opmsgtime1;
                            opmsgmsg1 = alarmmsg1.opm_his.data1.alm_msg.ToString();
                            if (opmsgmsg1.Contains('\''))
                            {
                                opmsgmsg1.Replace("\'", "");
                            }
                            opmsgno1 = alarmmsg1.opm_his.data1.om_no.ToString();
                            String[] msgsplit = opmsgmsg1.Trim().Split(' ');
                            opmsgcode1 = msgsplit[0];
                            if (opmsgcode1.Contains('M'))
                            {
                                opmsgcode1 = opmsgcode1.Substring(1);
                            }
                        }

                        if (opmsgdate1 != null)
                        {
                            OD.MsgDesc = opmsgmsg1.ToUpper();
                            OD.MsgNo = opmsgno1.ToUpper();
                            OD.OccurredOn = Convert.ToDateTime(opmsgdatetime1).ToString("dd-MM-yyyy HH:mm:ss");
                            OpMsgDetailsList.Add(OD);
                        }
                    }
                    e_no = (ushort)(e_no - 1);
                    s_no = (ushort)(e_no - 1);
                    if (e_no == 0)
                    {
                        e_no = 0;
                        s_no = 0;
                    }
                }
            }
            stopret = Focas1.cnc_startophis(h);
        }

        //Operator Message History Type 2
        internal void formatinsertMsgHis2(out List<OpMsgHisDetails> OpMsgDetailsList)
        {
            var alarmmsg1 = new Focas1.ODBOMHIS();// Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
            Focas1.ODBOMIF msgnumber = new Focas1.ODBOMIF();
            var stopret = Focas1.cnc_stopomhis(h);
            int retNumOfMsgs = Focas1.cnc_rdomhisinfo(h, msgnumber);
            if (retNumOfMsgs == 1)
            {
                HistoryReadError(h, 1);
                retNumOfMsgs = Focas1.cnc_rdomhisinfo(h, msgnumber);
            }
            int currentnumber = msgnumber.om_sum;
            OpMsgDetailsList = new List<OpMsgHisDetails>();
            if (currentnumber != 0)
            {
                ushort msglent = (ushort)(4 + alarmmsg1.omhis1.om_msg.Length * 2); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                ushort s_no = 0;
                ushort e_no = 1;

                for (int i = 0; i < currentnumber; i++)
                {
                    OpMsgHisDetails OD = new OpMsgHisDetails();
                    int almsgyear = 0;
                    int alarmmsgret = Focas1.cnc_rdomhistry(h, s_no, ref e_no, alarmmsg1); // Fanuc Controller 32i/Oi-TD/Oi-MD/310iM
                    if (alarmmsgret == 0)
                    {
                        String opmsgdate1 = null, opmsgtime1 = null, opmsgdatetime1 = null, opmsgmsg1 = null, opmsgno1 = null, opmsgcode1 = null;
                        // saving date from operator msg history
                        if (alarmmsg1.omhis1.day.ToString() != "0")
                        {
                            almsgyear = alarmmsg1.omhis1.year;
                            if (almsgyear.ToString().Length == 2)
                            {
                                String year = "20" + almsgyear;
                                almsgyear = Convert.ToInt32(year);
                            }
                            else if (almsgyear.ToString().Length == 3)
                            {
                                String year = "2" + almsgyear;
                                almsgyear = Convert.ToInt32(year);
                            }
                            opmsgdate1 = almsgyear + "/" + alarmmsg1.omhis1.month.ToString() + "/" + alarmmsg1.omhis1.day.ToString();
                            opmsgtime1 = alarmmsg1.omhis1.hour.ToString() + ":" + alarmmsg1.omhis1.minute.ToString() + ":" + alarmmsg1.omhis1.second.ToString();
                            String Datessss = almsgyear + "-" + alarmmsg1.omhis1.month.ToString() + "-" + alarmmsg1.omhis1.day.ToString();
                            opmsgdatetime1 = Convert.ToDateTime(Datessss).ToString("yyyy-MM-dd") + " " + opmsgtime1;
                            opmsgmsg1 = alarmmsg1.omhis1.om_msg.ToString();
                            if (opmsgmsg1.Contains('\''))
                            {
                                opmsgmsg1.Replace("\'", "");
                            }
                            opmsgno1 = alarmmsg1.omhis1.om_no.ToString();
                            String[] msgsplit = opmsgmsg1.Trim().Split(' ');
                            opmsgcode1 = msgsplit[0];
                            if (opmsgcode1.Contains('M'))
                            {
                                opmsgcode1 = opmsgcode1.Substring(1);
                            }
                        }

                        if (opmsgdate1 != null)
                        {
                            OD.MsgDesc = opmsgmsg1.ToUpper();
                            OD.MsgNo = opmsgno1.ToUpper();
                            OD.OccurredOn = Convert.ToDateTime(opmsgdatetime1).ToString("dd-MM-yyyy HH:mm:ss");
                            OpMsgDetailsList.Add(OD);
                        }
                    }
                    s_no = (ushort)(s_no + 1);
                }
            }
            stopret = Focas1.cnc_startomhis(h);
        }

        internal void HistoryReadError(ushort h, int readstart)
        {
            short ReadHisPar = 3112;
            var PreadHisParMe = new Focas1.IODBPSD_1();
            var readHisParMeBU = new Focas1.IODBPSD_1();

            int ctti = Focas1.cnc_rdparam(h, ReadHisPar, 0, 4 + 8 * Focas1.MAX_AXIS, PreadHisParMe);

            var ProgParRead = PreadHisParMe.cdata;
            readHisParMeBU = PreadHisParMe;
            if (readstart == 1)
            {
                PreadHisParMe.cdata = 4;
            }
            else if (readstart == 0)
            {
                PreadHisParMe.cdata = 0;
            }

            ctti = Focas1.cnc_wrparam(h, 4 + 1 * 1, PreadHisParMe);
        }
    }

    #endregion

    #region Heidenhain Controller

    public class VirtualHMIHDN
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        ushort port;
        String ip;
        String InventoryNo;
        int timeout;
        ushort h; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        short ret; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM

        public VirtualHMIHDN(String IP, String InvNo) //String IP
        {
            port = 5000;
            ip = IP;// "192.168.0.30";
            InventoryNo = InvNo;
            timeout = 3;

            ret = Focas1.cnc_allclibhndl3(ip, port, timeout, out h); // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        }

    }

    #endregion
}