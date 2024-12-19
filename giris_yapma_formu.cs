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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace proje_arayüzü
{
    public partial class giris_yapma_formu : Form
    {
        private int loginAttempts = 0;
        private const int MAX_LOGIN_ATTEMPTS = 3;

        public giris_yapma_formu()
        {
            InitializeComponent();
        }

        SqlConnection baglanti = new SqlConnection(@"Data Source=SEVVAL\SQLEXPRESS01;Initial Catalog=Kisiler;Integrated Security=True");
        barkod_Form barkod_Form = new barkod_Form();

        private void button1_Click(object sender, EventArgs e)
        {
            // Check for empty fields
            if (String.IsNullOrEmpty(textBox1.Text) || String.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Kullanıcı adı ve şifre boş bırakılamaz.");
                return;
            }

            try
            {
                baglanti.Open();
                SqlCommand komut = new SqlCommand("select * from uye_ol where kullanici_adi = @kullaniciAdi and sifre = @sifre", baglanti);
                komut.Parameters.AddWithValue("@kullaniciAdi", textBox1.Text);
                komut.Parameters.AddWithValue("@sifre", textBox2.Text);
                SqlDataReader dr = komut.ExecuteReader();

                if (dr.Read())
                {
                    // Successful login
                    listBox1.Items.Clear(); // Önceki öğeleri temizle
                    listBox1.Items.Add(dr["yas"].ToString());
                    listBox1.Items.Add(dr["hastalık"].ToString());
                    listBox1.Items.Add(dr["alerji"].ToString());
                    listBox1.Items.Add(dr["boy"].ToString());
                    listBox1.Items.Add(dr["kilo"].ToString());
                    listBox1.Items.Add(dr["cinsiyet"].ToString());
                    listBox1.Items.Add(dr["vegan"].ToString());
                    listBox1.Items.Add(dr["vejeteryan"].ToString());
                    listBox1.Items.Add(dr["hamile"].ToString());

                    foreach (var item in listBox1.Items)
                    {
                        barkod_Form.listBox2.Items.Add(item.ToString());
                    }

                    // Reset login attempts on successful login
                    loginAttempts = 0;

                    this.Hide();
                    barkod_Form.Show();

                    // Clear textboxes
                    textBox1.Clear();
                    textBox2.Clear();
                }
                else
                {
                    // Increment login attempts
                    loginAttempts++;

                    if (loginAttempts >= MAX_LOGIN_ATTEMPTS)
                    {
                        // Block login after 3 failed attempts
                        MessageBox.Show("Çok fazla hatalı giriş denemesi. Uygulama kapatılacaktır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    }
                    else
                    {
                        // Show remaining attempts
                        int remainingAttempts = MAX_LOGIN_ATTEMPTS - loginAttempts;
                        MessageBox.Show($"Kullanıcı adı ya da şifreniz yanlış. Kalan deneme hakkınız: {remainingAttempts}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    // Clear textboxes after each failed attempt
                    textBox1.Clear();
                    textBox2.Clear();
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
            finally
            {
                baglanti.Close();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.MaxLength = 15;
            textBox2.MaxLength = 15;
        }
    }
}