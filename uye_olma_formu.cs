using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlClient;


namespace proje_arayüzü
{
    public partial class uye_olma_formu : Form
    {
        public uye_olma_formu()
        {
            InitializeComponent();
        }
        SqlConnection baglanti = new SqlConnection(@"Data Source=SEVVAL\SQLEXPRESS01;Initial Catalog=Kisiler;Integrated Security=True");

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void uye_olma_formu_Load(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.MaxLength = 15;
            textBox2.MaxLength = 15;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void toolStripContainer1_ContentPanel_Load(object sender, EventArgs e)
        {

        }


        private void button2_Click(object sender, EventArgs e)
        {
            string sorgu = "insert into uye_ol(kullanici_adi,sifre,yas,hastalık,alerji,boy,kilo,cinsiyet,sigara,alkol,vegan,vejeteryan,hamile)values(@kullanici_adi,@sifre,@yas,@hastalık,@alerji,@boy,@kilo,@cinsiyet,@sigara,@alkol,@vegan,@vejeteryan,@hamile)";
            SqlCommand komut = new SqlCommand(sorgu, baglanti);
            komut.Parameters.AddWithValue("@kullanici_adi", textBox1.Text);
            komut.Parameters.AddWithValue("@sifre", textBox2.Text);
            komut.Parameters.AddWithValue("@yas", comboBox1.Text);
            komut.Parameters.AddWithValue("@hastalık", comboBox3.Text);
            komut.Parameters.AddWithValue("@alerji", comboBox4.Text);
            komut.Parameters.AddWithValue("@boy", textBox3.Text);
            komut.Parameters.AddWithValue("@kilo", textBox4.Text);
            komut.Parameters.AddWithValue("@cinsiyet", radioButton1.Checked ? "Erkek" : "Kadın");
            komut.Parameters.AddWithValue("@sigara", checkBox3.Checked);
            komut.Parameters.AddWithValue("@alkol", checkBox4.Checked);
            komut.Parameters.AddWithValue("@vegan", checkBox5.Checked);
            komut.Parameters.AddWithValue("@vejeteryan", checkBox6.Checked);
            komut.Parameters.AddWithValue("@hamile", checkBox7.Checked);

            
            try
            {
                baglanti.Open();
                komut.ExecuteNonQuery();
                MessageBox.Show("Kaydınız başarıyla oluşturulmuştur");
                giris_yapma_formu giris_yapma = new giris_yapma_formu();
                giris_yapma.listBox1.Items.Add(comboBox1.Text);
                giris_yapma.listBox1.Items.Add(comboBox3.Text);
                giris_yapma.listBox1.Items.Add(comboBox4.Text);
                giris_yapma.listBox1.Items.Add(textBox3.Text);
                giris_yapma.listBox1.Items.Add(textBox4.Text);
                giris_yapma.listBox1.Items.Add(radioButton1.Checked ? "Erkek" : "Kadın");
                giris_yapma.listBox1.Items.Add(checkBox5.Checked);
                giris_yapma.listBox1.Items.Add(checkBox6.Checked);
                giris_yapma.listBox1.Items.Add(checkBox7.Checked);

                this.Hide();
                giris_yapma.Show();

                
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Veritabanı hatası: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
            finally
            {

                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox4.Clear();
                comboBox1.SelectedIndex = -1;
                comboBox3.SelectedIndex = -1;
                comboBox4.SelectedIndex = -1;
                radioButton1.Checked = false;
                radioButton2.Checked = false;
                checkBox3.Checked = false;
                checkBox4.Checked = false;
                checkBox5.Checked = false;
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                baglanti.Close();
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox3.Text, out int value))
            {
                if (value > 220)
                {
                    textBox3.Text = "220";
                    textBox3.SelectionStart = textBox3.Text.Length; // İmleci sonuna taşı
                }
            }
            else
            {
                textBox3.Text = string.Empty; // Geçersiz giriş varsa temizle
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox4.Text, out int value))
            {
                if (value > 200)
                {
                    textBox4.Text = "200";
                    textBox4.SelectionStart = textBox4.Text.Length; // İmleci sonuna taşı
                }
            }
            else
            {
                textBox4.Text = string.Empty; // Geçersiz giriş varsa temizle
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}