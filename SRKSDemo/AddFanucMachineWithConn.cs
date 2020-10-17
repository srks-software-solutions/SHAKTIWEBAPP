using System;
using DNC;

using System.Threading;
using SRKSDemo.Server_Model;

namespace SRKSDemo
{
    public class AddFanucMachineWithConn
    {
        #region Variables
        //i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        ushort port;
        String ip;
        int timeout;
        ushort h; // Fanuc Controller 32i/Oi-TD/16i-MB/18i-TB/Oi-MC/Oi-TC/Oi-MD/Oi-MC Slim/21i-TB/310iM
        #endregion

        public AddFanucMachineWithConn(String IP) //String IP
        {
            port = 8193;
            ip = IP;
            timeout = 10;
            //int Conn;
            //AddFanucMac(out Conn, out ContType, out Axissss, out CurrentAxis, out ModType);
        }

        internal void AddFanucMac(out int connected, out String ControllerType, out int NoOfAxis, out int CurrentControlAxis, out String ModelType)
        {
            connected = 0;
            ControllerType = "";
            NoOfAxis = 0;
            CurrentControlAxis = 0;
            ModelType = "";
            Focas1.focas_ret retallclibhndl3 = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
            if (retallclibhndl3 == Focas1.focas_ret.EW_OK)
            {
                Focas1.ODBSYS MacInfo = new Focas1.ODBSYS();
                short SysInfoRet = Focas1.cnc_sysinfo(h, MacInfo);
                if (SysInfoRet == 0)
                {
                    connected = 1;
                    ControllerType = (MacInfo.cnc_type[0].ToString() + MacInfo.cnc_type[1].ToString()).ToString().Trim();
                    NoOfAxis = MacInfo.max_axis;
                    ModelType = (MacInfo.mt_type[0].ToString() + MacInfo.mt_type[1].ToString()).ToString().Trim();
                    //NoOfAxis = Convert.ToInt32(MacInfo.axes.ToString());
                    CurrentControlAxis = Convert.ToInt32((MacInfo.axes[0].ToString() + MacInfo.axes[1].ToString()).ToString().Trim());
                    String MacSeries = MacInfo.series.ToString().Trim();
                    String MacVersion = MacInfo.version.ToString().Trim();
                }
            }
        }

        internal void setmachinelock(bool LockStatus, ushort LockDBit = 0, ushort MsgDBit = 0, ushort UnLockDBit = 0)
        {
            Focas1.focas_ret retallclibhndl3 = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
            if (LockStatus)
            {
                //UnLocking D Bit Parameters
                var rdpmcdataUnLockBit = new Focas1.IODBPMC0();
                short adr_typeUn = 9;
                short data_typeUn = 0;
                ushort s_numberUn = UnLockDBit;
                ushort e_numberUn = UnLockDBit;
                ushort lengthUn = 9;

                short rdretUnLock = Focas1.pmc_rdpmcrng(h, adr_typeUn, data_typeUn, s_numberUn, e_numberUn, lengthUn, rdpmcdataUnLockBit);

                var wrpmcdataUn = rdpmcdataUnLockBit;
                wrpmcdataUn.cdata[0] = 0;
                for (int i = 0; i < 100; i++)
                {
                    short wrret = Focas1.pmc_wrpmcrng(h, lengthUn, wrpmcdataUn);
                    if (wrret == 0)
                    {
                        break;
                    }
                }

                //Locking D Bit Parameters
                var rdpmcdataLockBit = new Focas1.IODBPMC0();
                short adr_type = 9;
                short data_type = 0;
                ushort s_number = LockDBit;
                ushort e_number = LockDBit;
                ushort length = 9;

                short rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);

                //IdleMessage D Bit Parameters
                var rdpmcdataIdleBit = new Focas1.IODBPMC0();
                short adr_typeIdle = 9;
                short data_typeIdle = 0;
                ushort s_numberIdle = MsgDBit;
                ushort e_numberIdle = MsgDBit;
                ushort lengthIdle = 9;

                short rdretIDLE = Focas1.pmc_rdpmcrng(h, adr_typeIdle, data_typeIdle, s_numberIdle, e_numberIdle, lengthIdle, rdpmcdataIdleBit);

                //Machine LOCK D Bit
                var wrpmcdata = rdpmcdataLockBit;
                wrpmcdata.cdata[0] = 1;
                for (int i = 0; i < 10; i++)
                {
                    short wrretLOCK = Focas1.pmc_wrpmcrng(h, length, wrpmcdata);
                }

                if (MsgDBit != 0)
                {
                    //IDLE Message D Bit
                    var wrpmcdataIDLE = rdpmcdataIdleBit;
                    wrpmcdataIDLE.cdata[0] = 1;
                    for (int i = 0; i < 10; i++)
                    {
                        short wrretIDLE = Focas1.pmc_wrpmcrng(h, length, wrpmcdataIDLE);
                    }

                    wrpmcdataIDLE.cdata[0] = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        short wrretIDLE = Focas1.pmc_wrpmcrng(h, length, wrpmcdataIDLE);
                    }
                }

                Thread.Sleep(2000);
                //Lock D Bit reverted back to OFF State
                rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);
                var wrpmcdata1 = rdpmcdataLockBit;
                wrpmcdata1.cdata[0] = 0;
                for (int i = 0; i < 10; i++)
                {
                    short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdata1);
                }
            }
            else
            {
                //UnLocking D Bit Parameters
                var rdpmcdataLockBit = new Focas1.IODBPMC0();
                short adr_type = 9;
                short data_type = 0;
                ushort s_number = LockDBit;
                ushort e_number = LockDBit;
                ushort length = 9;

                short rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);

                var wrpmcdata1 = rdpmcdataLockBit;
                wrpmcdata1.cdata[0] = 0;
                for (int i = 0; i < 100; i++)
                {
                    short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdata1);
                    if (wrret == 0)
                    {
                        break;
                    }
                }
            }
        }

        internal void SetMachineUnlock(ushort UnLockDBit, ushort LockDBit)
        {
            Focas1.focas_ret retallclibhndl3 = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(ip, port, timeout, out h);
            //Locking D Bit Parameters
            var rdpmcdataLockBit = new Focas1.IODBPMC0();
            short adr_type = 9;
            short data_type = 0;
            ushort s_number = LockDBit;
            ushort e_number = LockDBit;
            ushort length = 9;

            short rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);

            var wrpmcdataLock = rdpmcdataLockBit;
            wrpmcdataLock.cdata[0] = 0;
            for (int i = 0; i < 10; i++)
            {
                short wrret = Focas1.pmc_wrpmcrng(h, length, wrpmcdataLock);
            }

            //UnLocking D Bit Parameters
            var rdpmcdataUnLockBit = new Focas1.IODBPMC0();
            short adr_typeUn = 9;
            short data_typeUn = 0;
            ushort s_numberUn = UnLockDBit;
            ushort e_numberUn = UnLockDBit;
            ushort lengthUn = 9;

            short rdretUnLock = Focas1.pmc_rdpmcrng(h, adr_typeUn, data_typeUn, s_numberUn, e_numberUn, lengthUn, rdpmcdataUnLockBit);

            var wrpmcdataUn = rdpmcdataUnLockBit;
            wrpmcdataUn.cdata[0] = 1;
            for (int i = 0; i < 10; i++)
            {
                short wrret = Focas1.pmc_wrpmcrng(h, lengthUn, wrpmcdataUn);
            }

            Thread.Sleep(2000);
            //Lock D Bit reverted back to OFF State
            rdretUnLock = Focas1.pmc_rdpmcrng(h, adr_typeUn, data_typeUn, s_numberUn, e_numberUn, lengthUn, rdpmcdataUnLockBit);
            var wrpmcdata1 = rdpmcdataUnLockBit;
            wrpmcdata1.cdata[0] = 0;
            for (int i = 0; i < 10; i++)
            {
                short wrret = Focas1.pmc_wrpmcrng(h, lengthUn, wrpmcdata1);
            }

        }
    }
}