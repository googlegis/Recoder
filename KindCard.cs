using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;

using System.Windows.Forms;

namespace Registrar
{
    public partial class KindCard : Form
    {
        public KindCard()
        {
            InitializeComponent();
            ShowNetworkInterfaceMessage();
        }


        public void ShowNetworkInterfaceMessage()
        {

            this.listBox1.Items.Clear();

            NetworkInterface[] fNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in fNetworkInterfaces)
            {
                #region " 网卡类型 "
                string fCardType = "未知网卡";
                string fRegistryKey = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + adapter.Id + "\\Connection";
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(fRegistryKey, false);
                if (rk != null)
                {
                    // 区分 PnpInstanceID 
                    // 如果前面有 PCI 就是本机的真实网卡
                    // MediaSubType 为 01 则是常见网卡，02为无线网卡。
                    string fPnpInstanceID = rk.GetValue("PnpInstanceID", "").ToString();
                    int fMediaSubType = Convert.ToInt32(rk.GetValue("MediaSubType", 0));
                    if (fPnpInstanceID.Length > 3 &&
                        fPnpInstanceID.Substring(0, 3) == "PCI")
                        fCardType = "物理网卡";
                    else if (fMediaSubType == 1)
                        fCardType = "虚拟网卡";
                    else if (fMediaSubType == 2)
                        fCardType = "无线网卡";
                }
                #endregion
                #region " 网卡信息 "
                this.listBox1.Items.Add("-----------------------------------------------------------");
                this.listBox1.Items.Add("-- " + fCardType);
                this.listBox1.Items.Add("-----------------------------------------------------------");
                this.listBox1.Items.Add("Id .................. : "+ adapter.Id); // 获取网络适配器的标识符
                this.listBox1.Items.Add("Name ................ : " + adapter.Name); // 获取网络适配器的名称
                this.listBox1.Items.Add("Description ......... : " + adapter.Description); // 获取接口的描述
                this.listBox1.Items.Add("Interface type ...... : " + adapter.NetworkInterfaceType); // 获取接口类型
                this.listBox1.Items.Add("Is receive only...... : " + adapter.IsReceiveOnly); // 获取 Boolean 值，该值指示网络接口是否设置为仅接收数据包。
                this.listBox1.Items.Add("Multicast............ : " + adapter.SupportsMulticast); // 获取 Boolean 值，该值指示是否启用网络接口以接收多路广播数据包。
                this.listBox1.Items.Add("Speed ............... : " + adapter.Speed); // 网络接口的速度
                this.listBox1.Items.Add("Physical Address .... : " + adapter.GetPhysicalAddress().ToString()); // MAC 地址
                IPInterfaceProperties fIPInterfaceProperties = adapter.GetIPProperties();
                UnicastIPAddressInformationCollection UnicastIPAddressInformationCollection = fIPInterfaceProperties.UnicastAddresses;
                foreach (UnicastIPAddressInformation UnicastIPAddressInformation in UnicastIPAddressInformationCollection)
                {
                    if (UnicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                        this.listBox1.Items.Add("Ip Address .......... : " + UnicastIPAddressInformation.Address); // Ip 地址
                }
                this.listBox1.Items.Add("");
                #endregion
            }
            //Console.ReadKey();
        }
    }
}
