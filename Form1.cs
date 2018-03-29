using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Xml;

namespace Registrar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var register = new Register();

            //if (SentinelKeyForNet.CheckLicense())
            //    this.textBox1.Text = @"1" + register.GetHardInfo();
            //else
            //{
                this.textBox1.Text = @"0" + register.GetHardInfo();
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string tempstring = this.textBox1.Text.Trim();
            if(tempstring.Length==0) return;
            if(tempstring.Length<65)
            {
                MessageBox.Show(@"请用较低版本注册机注册此机器吗");
                return;
            }

            //if(!this.chkdog.Checked)
            //{
            //    if(MessageBox.Show(@"此机器码信息表示不需要检测硬件狗信息，你确认要继续生成license吗？",@"系统提示--不检测硬件狗信息",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning) == DialogResult.Cancel)
            //        return;
            //}
            
            string checkdog = "SO_" + tempstring.Substring(0, 1);
           
            #region 机器码
            string computerCode =tempstring.Substring(1);
            #endregion
            
            #region  RSA加密
            byte[] b = GetRSAKey(computerCode);
            #endregion

            #region 时间限制
            var dateTime = DateTime.Now;
            var dtnew = new DateTime();

            var diff = 60;

            if (this.radioButton3.Checked) //直接使用时间差
            {
                Int32 dI = Convert.ToInt32(this.textBox3.Text.Trim());
                dtnew = dateTime.AddDays(dI);
                diff = dI;
            }
            else if (this.radioButton4.Checked) //选取时间值
            {
                dtnew = this.dateTimePicker1.Value;
                diff = dtnew.Subtract(DateTime.Now).Days;
            }

            string tn = System.DateTime.Now.ToString(CultureInfo.InvariantCulture);
            string tm = dtnew.ToUniversalTime().ToString(CultureInfo.InvariantCulture);
            //版本  到期时间  dontcrackit
            string time = diff.ToString(CultureInfo.InvariantCulture) + "_" + Convert.ToDateTime(tm).ToString(CultureInfo.InvariantCulture) +"_"+ tn +"_if_you_are_good_boy_do_not_crack_it ";
            #endregion

     
            string file = Application.StartupPath + @"\License.lic";
            if (File.Exists(file))
                File.Delete(file);
            var streamWriter = new StreamWriter(file, false);
            //                             机器码             时间限制            加密                    检测硬狗标示
            string content = EncodeBase64(computerCode + "`" + time + "`" + Convert.ToBase64String(b) +"`"+checkdog);
            streamWriter.WriteLine(content);
            streamWriter.Close();

            MessageBox.Show(@"创建注册文件完成！", @"提示", MessageBoxButtons.OKCancel);

        }

        private byte[] GetRSAKey(string machineCode)
        {
            const string prikey ="<RSAKeyValue><Modulus>rQinvSz2Wv8DUm1DG5OazzrQu79HtIYdpvOYN5ifUQbNfhh6lRccHF5WUCDMpyJY5LAlf7G2aY/jpHAVzqDGjRyV7MQGUVNUJplUxqXd1uyGZhsvrasdmf5pTX2N00/KtbKpRUzkQMcqCpmK2Q4yGMr0drMnT7wv9l8OrpPkCR0=</Modulus><Exponent>AQAB</Exponent><P>tjV+ded/RtV0vC0dJx7wDGp7CQYpEj1GaqsTrT6ihaNff4LOfFmYspRmk7/vrXJhixGLtTk1N7ewYGv1fetfzw==</P><Q>8xvthggFxSEo0qBRZHsIulOKvqSMDLYiGK8+ldUvqYy2pCwsgp4KjBVCRge9/wA/hd+awmntqig1FHsStjm3Uw==</Q><DP>OHbJUpZDjdrWCv5b+2SN9PsGV5yOG7XbXXDYbyZqzMj87hHGFSjatfRg+UZQasp4SdVNGwK4aCTHRooOEFBhZw==</DP><DQ>YnrJuSW+0KAiHVB8KCv+2RvGdHvLj8qn/T/gJmn5qMErq02Jqk/DDgP+mMfCG25KTTzLQD4Q3ID1H5rLda3jqQ==</DQ><InverseQ>qnm8C44E6593i3kRPk0DCjLESCyFvt9NOHTmrqrGh7aC2ZFO+itIlisxY1UdC1Tm1mLpAHvCB/tdzrSuvHzc/g==</InverseQ><D>GUJdMjmBERGPC5ZVqI3omH3OgMnQjuLRK1D+FIecIjjrAJBUPLVt7ho7YWEYXwGdlmy8XKK1rT1LDvMpy8sekLbX9jC/WzzDw8Cct1jGNYSwdpdanunfMu/y5OvJaaa3y7kUgopTkz+c9igHNqsDR1gJKco2ZlHMRFvaQ+hQOIU=</D></RSAKeyValue>";
            using (var rsa = new RSACryptoServiceProvider())
            {

                rsa.FromXmlString(prikey);
                var f = new RSAPKCS1SignatureFormatter(rsa);
                f.SetHashAlgorithm("SHA1");
                byte[] source = System.Text.Encoding.ASCII.GetBytes(machineCode);
                var sha = new SHA1Managed();
                byte[] result = sha.ComputeHash(source);
                byte[] b = f.CreateSignature(result);

                return b;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
            System.Environment.Exit(0);
        }

        /// <summary> 
        /// Base64加密 
        /// </summary> 
        /// <param name="codeName">加密采用的编码方式</param> 
        /// <param name="source">待加密的明文</param> 
        /// <returns></returns> 
        public static string EncodeBase64(Encoding encode, string source)
        {
            string rs = string.Empty;
            byte[] bytes = encode.GetBytes(source);
            try
            {
                rs = Convert.ToBase64String(bytes);
            }
            catch
            {
                rs = source;
            }
            return rs;
        }

        /// <summary> 
        /// Base64加密，采用utf8编码方式加密 
        /// </summary> 
        /// <param name="source">待加密的明文</param> 
        /// <returns>加密后的字符串</returns> 
        public static string EncodeBase64(string source)
        {
            return EncodeBase64(Encoding.UTF8, source);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.radioButton3.Checked = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {   
            this.textBox3.Enabled = this.radioButton3.Checked;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            this.dateTimePicker1.Enabled = this.radioButton4.Checked;
        }

        private void Writelog(string msg)
        {
            try
            {
                StreamWriter filestr = new StreamWriter(Application.StartupPath + "\\log.log", true);

                msg = System.DateTime.Now.ToString() + ":" + msg;
                filestr.WriteLine(msg);
                filestr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string tempstring = this.textBox1.Text.Trim();
            if(tempstring.Length==0) return;

            if(tempstring.Substring(0,1)=="0")
            {
                this.chkdog.Checked = false;
            }
            else if(tempstring.Substring(0,1)=="1")
            {
                this.chkdog.Checked = true;
            }
        }

        private void chkdog_Click(object sender, EventArgs e)
        {
            if (chkdog.Checked)
            {
                if (MessageBox.Show(@"是否强制在license信息中增加硬件狗检测信息？", @"系统提示---添加硬件狗检测", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    this.textBox1.Text = @"1" + this.textBox1.Text.Trim().Substring(1, this.textBox1.Text.Trim().Length - 1);
                    this.chkdog.Checked = true;
                }
                else
                {
                    this.chkdog.Checked = false;
                }
            }
            else
                if (MessageBox.Show(@"是否强制在license信息中删除硬件狗检测信息？", @"系统提示--删除硬件狗检测", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    this.textBox1.Text = "0" + this.textBox1.Text.Trim().Substring(1, this.textBox1.Text.Trim().Length - 1);
                    this.chkdog.Checked = false;
                }
                else
                    this.chkdog.Checked = true;
        }

    }
}
