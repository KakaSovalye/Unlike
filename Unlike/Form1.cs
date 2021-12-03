using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using DevExpress.Utils;
namespace Unlike
{
    public partial class Form1 : Form
    {
        private Bitmap Master;
        private Bitmap Product;
        private Rectangle rect;
        private Rectangle rect2;
        private System.Drawing.Imaging.BitmapData bmpData;
        private System.Drawing.Imaging.BitmapData bmpData2;
        private IntPtr ptr;
        private int intbytes;
        private int intbytes2;
        private byte[] rgbValues;
        private byte[] rgbValuesProduct;
        private bool boolHata = false;
        
        public Form1()
        {
            InitializeComponent();
            memMSG.Properties.ReadOnly = true;
            btYarat.Enabled = false;
            btRecover.Enabled = false;
        }

        private string Reverse2(string x)
        {
            char[] charArray = new char[x.Length];
            int len = x.Length - 1;
            for (int i = 0; i <= len; i++)
                charArray[i] = x[len - i];
            return new string(charArray);
        }

        private string Bitter(string strmessage)
        {
            string res = "";
            char[] chars = strmessage.ToCharArray();
            for (int j = 0; j < chars.Length; j++)
            {
                BitArray A = new BitArray(BitConverter.GetBytes(chars[j]));
                
                for (int i = 0;i<A.Count; i++)
                {
                    if (A[i] == true)
                        res = res + "1";
                    else
                        res = res + "0";
                }
            }
            return res+"2";//Sonlandirici 2 eklemesi.
        }

        private BitArray Msg2BitArr(string bits)
        {
            char[] bitsC = bits.ToCharArray();
            BitArray realBits = new BitArray(bitsC.Length);

            for (int i = 0; i < bitsC.Length; i++)
            {
                if (bitsC[i] == '1')
                    realBits[i] = true;
                else
                    realBits[i] = false;
            }
            return realBits;

        }

        private byte[] Bits2Bytes(BitArray bits)
        {
            byte[] bytes = new byte[bits.Length];
            bits.CopyTo(bytes, 0);
            return bytes;
        }
        private void LockUnlockBitsExample()
        {           
            
            // Yeni bitmap yarat.

            if (openFileDialog1.ShowDialog()== DialogResult.Cancel)
                return;
            WaitDialogForm wait = new WaitDialogForm("Resim Yükleniyor", "Lütfen bekleyiniz.");
            string strMasterPath = openFileDialog1.FileName;
            Master = new Bitmap(strMasterPath);
            pictureBox2.Image = Master;
            
            
            // Bitmap'in bitlerini kilitle.  
            rect = new Rectangle(0, 0, Master.Width, Master.Height);
            

            bmpData =
                Master.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                Master.PixelFormat);
                       

            // İlk satırın adresini al.
            ptr = bmpData.Scan0;

            // Bitmap'ın byte'larını bir diziye çek.
            intbytes  = bmpData.Stride * Master.Height;

            //int bytes2 = bmpData.Stride * bmp.Height;
            rgbValues = new byte[intbytes];

            // RGB değerlerini diziye kopyala.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, intbytes);

            int yazilabilir = 0;
            for (int i = 0; i < rgbValues.Length; i++)
            {
                if (Convert.ToInt32(rgbValues[i].ToString()) < 254)
                    yazilabilir++;
            }
            //Maksimum mesaj uzunluğu 
            txtMaxMsgLen.Text = (yazilabilir / 16).ToString();

            
            Master.UnlockBits(bmpData);
                       
           wait.Dispose();

            

        }

        private void FotoMesajla(string msgBits)
        {
            WaitDialogForm wait = new WaitDialogForm("Mesaj gizlenip yeni resim yaratılıyor.","Lütfen bekleyiniz.");
            Product = new Bitmap(openFileDialog1.FileName);
            Graphics g = Graphics.FromImage(Product);
            
            int msglen = 0;

            for (int counter = 0; counter < rgbValues.Length; counter += 1)
            {
                int ex = (int)rgbValues[counter];
                
                if (ex < 254)
                {
                    if (msglen<msgBits.Length)
                    {
                        if (msgBits[msglen] == '1')
                        {
                            ex = ex + 1;
                            rgbValues[counter] = (byte)ex;                            
                        }
                        else if (msgBits[msglen] == '2')
                        {
                            ex = ex + 2;
                            rgbValues[counter] = (byte)ex;                            
                        }
                        msglen = msglen + 1;
                    }
                }
            }


            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, intbytes);

            g.DrawImage(Master, rect);


            pictureBox1.Image = Product;
            wait.Dispose();
            saveFileDialog1.ShowDialog();
            try
            {
                pictureBox1.Image.Save(saveFileDialog1.FileName);
            }
            catch 
            {
                MessageBox.Show("Resim kaydı sırasında hata oluştu. Dosyanın kullanımda olmadığından emin olunuz.");
                boolHata = true;
                
            }
        }

        private string Bytes2Mesage(Byte[] bytes)
        {
            string msg = null;
            char msgC;

            for (int i = 0; i < bytes.Length; i = i + 2)
            {
                msgC = Convert.ToChar(bytes[i]);
                msg = msg + msgC.ToString();
            }
            return msg;
        }


        private void simpleButton1_Click(object sender, EventArgs e)
        {
            ///Bir yaratıcı olacak. image'a 8 bitlik veri yazacak. Her pixel 1 bit. Her pixelden 1 bit düşürecek. Eğer siyaha geldi
            /// ise ****burasına düşün. Yeni resimi oluşturacak. Daha sonra orjinali ile karşılaştırıp farklarınından şifreli veriyi
            /// çıkaracak bir de recons olacak.

            if (Convert.ToInt32(txtMsgLen.Text == "" ? "0" : txtMsgLen.Text) > Convert.ToInt32(txtMaxMsgLen.Text==""?"0":txtMaxMsgLen.Text))
            {
                MessageBox.Show(txtMaxMsgLen.Text+"'den daha uzun mesaj saklayamazsınız.\nYa mesajınızı kısaltın ya da farklı resim seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            pictureBox1.Image = null;
            FotoMesajla(Bitter(memMSG.Text));

            memoEdit1.Text = Bitter(memMSG.Text);
            BitArray bits = Msg2BitArr(memoEdit1.Text);
            byte[] msginbytes = Bits2Bytes(bits);            
            memoEdit2.Text = Bytes2Mesage(msginbytes);

            if (!boolHata)
                MessageBox.Show("Mesajınız resime başarıyla gizlendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                MessageBox.Show("Mesajın gizlenmesi başarısız oldu.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                boolHata = false;
            }
            
            txtMsgLen.Text = "0";
            memMSG.Text = "";
            memoEdit2.Text = "";
            memoEdit1.Text = "";
            btYarat.Enabled = false;
            btRecover.Enabled = false;
            
        }

        private void btMImageLoad_Click(object sender, EventArgs e)
        {
            memMSG.Properties.ReadOnly = false;
            pictureBox1.Image = null;
            LockUnlockBitsExample();
            btYarat.Enabled = true;
            btRecover.Enabled = true;

        }

        private void UrunYukle()
        {
            if (openFileDialog2.ShowDialog() == DialogResult.Cancel)
            {
                boolHata = true;
                return;
            }
            string strProductPath = openFileDialog2.FileName;
            Product = new Bitmap(strProductPath);
            pictureBox1.Image = Product;

            rect2 = new Rectangle(0, 0, Product.Width, Product.Height);
            
            bmpData2 =
                Product.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                Product.PixelFormat);

            ptr = bmpData2.Scan0;

            intbytes2 = bmpData2.Stride * Product.Height;

            rgbValuesProduct = new byte[intbytes2];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValuesProduct, 0, intbytes2);
            
            Product.UnlockBits(bmpData2);
        }

        private string MesajiAl()
        {
            string msgbits="";
            if (rgbValuesProduct.Length != rgbValues.Length)
            {
                MessageBox.Show("Resim bilgileri tutmuyor!\nYüklediğiniz farklı bir ana resimin ürünü olabilir mi?", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            else
            {
                int mV = 0, pV = 0, i=0;
                WaitDialogForm wait = new WaitDialogForm("Resim taranıyor.\nBu işlem uzun sürebilir.", "Lütfen bekleyiniz.");
                while (i < rgbValues.Length && (int)rgbValuesProduct[i] != (int)rgbValues[i]+2)
                {
                    mV = (int)rgbValues[i];
                    pV = (int)rgbValuesProduct[i];
                    if (mV < 254)
                    {
                        if (pV > mV)
                        {
                            msgbits = msgbits + "1";
                        }
                        else
                        {
                            msgbits = msgbits + "0";
                        }
                    }
                    i++;

                }
                wait.Dispose();                

            }
            return msgbits;
        }

        private void btRecover_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName == "" || openFileDialog1.FileName == null)
            {
                MessageBox.Show("Lütfen önce ana resmi yükleyiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            openFileDialog2.FileName = null;
            txtMsgLen.Text = "0";
            memMSG.Text = "";
            memoEdit2.Text = "";
            memoEdit1.Text = "";
            pictureBox1.Image = null;
            
            
            UrunYukle();

            if (boolHata)
                return;
                
            string msgbits = MesajiAl();
            if (msgbits == "")
                    return;
            WaitDialogForm wait = new WaitDialogForm("Gizlenmiş mesaj çıkarılıyor", "Lütfen bekleyiniz.");
            try
            {
                BitArray bits = Msg2BitArr(msgbits);
                byte[] msginbytes = Bits2Bytes(bits);
                memoEdit2.Text = Bytes2Mesage(msginbytes);
                wait.Dispose();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Resim okunurken hata oluştu.\nHata :" + ex.Message);
                btRecover.Enabled = false;
                wait.Dispose();
            }
            btRecover.Enabled = false;
            btYarat.Enabled = false;
            
        }

        private void memMSG_EditValueChanged(object sender, EventArgs e)
        {
            
        }

        private void memMSG_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            txtMsgLen.Text = memMSG.Text.Length.ToString();
        }

        private void txtMsgLen_EditValueChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(txtMsgLen.Text == "" ? "0" : txtMsgLen.Text) > Convert.ToInt32(txtMaxMsgLen.Text == "" ? "0" : txtMaxMsgLen.Text))
            {
                txtMsgLen.Properties.Appearance.ForeColor = Color.Red;
            }
        }

        private void btAbout_Click(object sender, EventArgs e)
        {
            About abt = new About();
            abt.MdiParent = this.MdiParent;
            abt.ShowDialog();
        }
    }
}
