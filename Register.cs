using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace Registrar
{
    internal class Register
    {
        #region   获取cpu序列号   硬盘ID   网卡硬地址

        ///   <summary> 
        ///   获取cpu序列号     
        ///   </summary> 
        ///   <returns> string </returns> 
        public string GetCpuInfo()
        {
            string cpuInfo = " ";
            var cimobject = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = cimobject.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
            }
            return cpuInfo;
        }

        ///   <summary> 
        ///   获取硬盘ID     
        ///   </summary> 
        ///   <returns> string </returns> 
        public string GetHDid()
        {
            string HDid = "";
            var cimobject1 = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            foreach (ManagementObject mo in moc1)
            {
                HDid = (string) mo.Properties["Model"].Value;
            }
            return HDid;
        }

        ///   <summary> 
        ///   获取物理网卡硬件地址 
        ///   </summary> 
        ///   <returns> string </returns> 
        public string GetMoAddress()
        {

            ManagementObjectSearcher s = new ManagementObjectSearcher(
  @"SELECT NetConnectionStatus FROM Win32_NetworkAdapter WHERE NetConnectionStatus=2 AND PNPDeviceID LIKE 'PCI%'");

            string moAddress = " ";

            NetworkInterface[] fNetWorkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in fNetWorkInterfaces)
            {
                string fRegistryKey =
                    "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + adapter.Id +
                    "\\Connection";
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(fRegistryKey, false);
                if (rk != null)
                {
                    string fPnpInstanceId = rk.GetValue("PnpInstanceID", "").ToString();
                    //int fMediaSubType = Convert.ToInt32(rk.GetValue("MediaSubType", 0));
                    if (fPnpInstanceId.Length > 3 &&
                        fPnpInstanceId.Substring(0, 3) == "PCI")
                    {
                        moAddress = moAddress.Trim() + adapter.GetPhysicalAddress();
                    }
                }
            }
            return moAddress;
        }

        #endregion

        [DllImport("kernel32.dll")]
        private static extern int GetVolumeInformation(
            string lpRootPathName,
            string lpVolumeNameBuffer,
            int nVolumeNameSize,
            ref int lpVolumeSerialNumber,
            int lpMaximumComponentLength,
            int lpFileSystemFlags,
            string lpFileSystemNameBuffer,
            int nFileSystemNameSize
            );

        public string GetVolOf(string drvId)
        {
            const int maxFilenameLen = 256;
            int retVal = 0;
            int a = 0;
            int b = 0;
            string str1 = null;
            string str2 = null;

            int i = GetVolumeInformation(
                drvId + @":\",
                str1,
                maxFilenameLen,
                ref retVal,
                a,
                b,
                str2,
                maxFilenameLen
                );

            return retVal.ToString("x");
        }

        //
        //MD5加密函数
        //
        public string MD5(String str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.Default.GetBytes(str);
            byte[] result = md5.ComputeHash(data);
            String ret = "";
            for (int i = 0; i < result.Length; i++)
                ret += result[i].ToString("x").PadLeft(2, '0');
            return ret;
        }

        /// <summary>
        /// 获取当前硬件信息
        /// </summary>
        /// <returns></returns>
        public string GetHardInfo()
        {
            string cpumd5 = MD5(GetCpuInfo());
            string volmd5 = MD5(GetVolOf("c"));
            string Macmd5 = MD5(GetMoAddress());
            string mcstr = cpumd5 + volmd5;// +Macmd5;
            return mcstr;
        }
    }
}