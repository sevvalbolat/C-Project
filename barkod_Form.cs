using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using System.Linq;
using System.Collections.Generic;

namespace proje_arayüzü
{
    public partial class barkod_Form : Form
    {
        public barkod_Form()
        {
            InitializeComponent();
        }

        SqlConnection baglanti = new SqlConnection(@"Data Source=SEVVAL\SQLEXPRESS01;Initial Catalog=Kisiler;Integrated Security=True");

        FilterInfoCollection fico;
        VideoCaptureDevice vcd;

        private void barkod_form_Load(object sender, EventArgs e)
        {
            fico = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo f in fico)
            {
                comboBox1.Items.Add(f.Name);
                comboBox1.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            vcd = new VideoCaptureDevice(fico[comboBox1.SelectedIndex].MonikerString);
            vcd.NewFrame += Vcd_NewFrame;
            vcd.Start();
            timer1.Start();
        }

        private void Vcd_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                BarcodeReader brd = new BarcodeReader();
                Result sonuc = brd.Decode((Bitmap)pictureBox1.Image);
                if (sonuc != null)
                {
                    richTextBox1.Text = sonuc.Text;
                    timer1.Stop();
                    vcd.Stop();

                    try
                    {
                        string sorgu = "INSERT INTO barkod_tablosu (barkod_numarası) VALUES (@barkod_numarası)";
                        SqlCommand komut = new SqlCommand(sorgu, baglanti);
                        komut.Parameters.AddWithValue("@barkod_numarası", sonuc.Text);

                        string BarcodeId = richTextBox1.Text;

                        baglanti.Open();
                        komut.ExecuteNonQuery();

                        // Barkod numarasını veritabanında arama
                        SqlCommand command1 = new SqlCommand("SELECT * FROM urunler WHERE BarcodeId = @BarcodeId", baglanti);
                        command1.Parameters.AddWithValue("@BarcodeId", BarcodeId);

                        SqlDataReader reader1 = command1.ExecuteReader();

                        bool isThere = false;
                        List<string> ingredients = new List<string>();

                        if (reader1.Read())
                        {
                            isThere = true;
                            ingredients = reader1["İçindekiler"].ToString().Split(',').Select(i => i.Trim()).ToList();
                        }
                        else
                        {
                            MessageBox.Show("Ürün veritabanında bulunamadı.");
                        }
                        reader1.Close();

                        if (isThere)
                        {
                            // Yasaklı maddeleri toplamak için liste
                            List<string> restrictedItems = new List<string>();

                            // 1. Hastalıklardan dolayı tüketemeyeceği ürünler ile karşılaştırma
                            SqlCommand command2 = new SqlCommand("SELECT * FROM hastalıklar", baglanti);
                            SqlDataReader reader2 = command2.ExecuteReader();

                            while (reader2.Read())
                            {
                                List<string> forbiddenItems = reader2["Tüketemeyeceği_ürünler"].ToString().Split(',').Select(i => i.Trim()).ToList();
                                restrictedItems.AddRange(ingredients.Intersect(forbiddenItems));
                            }
                            reader2.Close();

                            // 2. Yaş aralığından dolayı tüketemeyeceği ürünler ile karşılaştırma
                            string selectedAgeRange = listBox2.SelectedItem?.ToString();
                            if (!string.IsNullOrEmpty(selectedAgeRange))
                            {
                                SqlCommand command3 = new SqlCommand("SELECT * FROM yas_araligi WHERE yas_araligi = @yas_araligi", baglanti);
                                command3.Parameters.AddWithValue("@yas_araligi", selectedAgeRange);
                                SqlDataReader reader3 = command3.ExecuteReader();

                                if (reader3.Read())
                                {
                                    List<string> ageRestrictedItems = reader3["tuketemeyecegi_urunler"].ToString().Split(',').Select(i => i.Trim()).ToList();
                                    restrictedItems.AddRange(ingredients.Intersect(ageRestrictedItems));
                                }
                                reader3.Close();
                            }

                            // 3. Alerjenler nedeniyle tüketilmemesi gereken ürünler ile karşılaştırma
                            string selectedAllergen = listBox2.SelectedItem?.ToString();
                            if (!string.IsNullOrEmpty(selectedAllergen))
                            {
                                SqlCommand command4 = new SqlCommand("SELECT * FROM alerjen_yiyecekler WHERE Alerjen_yiyecek = @Alerjen_yiyecek", baglanti);
                                command4.Parameters.AddWithValue("@Alerjen_yiyecek", selectedAllergen);
                                SqlDataReader reader4 = command4.ExecuteReader();

                                if (reader4.Read())
                                {
                                    List<string> allergenRestrictedItems = reader4["Alerjen_ürünler"].ToString().Split(',').Select(i => i.Trim()).ToList();
                                    restrictedItems.AddRange(ingredients.Intersect(allergenRestrictedItems));
                                }
                                reader4.Close();
                            }

                            // 4. Vücut Kütle İndeksi Hesaplama ve Karşılaştırma
                            if (listBox2.Items.Count >= 2)
                            {
                                // Assume first item is height, second item is weight
                                float boy = float.Parse(listBox2.Items[3].ToString());
                                float kilo = float.Parse(listBox2.Items[4].ToString());

                                double boyMetre = boy / 100.0;
                                double Vücut_k_e = kilo / (boyMetre * boyMetre);

                                // Modify the query to select the range where the calculated BMI falls
                                SqlCommand command5 = new SqlCommand(@"SELECT Tüketemeyeceği_ürünler FROM vücut_kutle_indeksi
          WHERE (@Vücut_k_e >= TRY_CAST(PARSENAME(REPLACE(Vücut_k_e, '–', '.'), 2) AS FLOAT)
            OR PARSENAME(REPLACE(Vücut_k_e, '–', '.'), 2) IS NULL)
          AND (@Vücut_k_e < TRY_CAST(PARSENAME(REPLACE(Vücut_k_e, '–', '.'), 1) AS FLOAT)
            OR PARSENAME(REPLACE(Vücut_k_e, '–', '.'), 1) IS NULL)
    ", baglanti);
                             
                                command5.Parameters.AddWithValue("@Vücut_k_e", Vücut_k_e);
                                SqlDataReader reader5 = command5.ExecuteReader();

                                while (reader5.Read())
                                {
                                    List<string> bmiRestrictedItems = reader5["Tüketemeyeceği_ürünler"].ToString().Split(',').Select(i => i.Trim()).ToList();
                                    restrictedItems.AddRange(ingredients.Intersect(bmiRestrictedItems));
                                }
                                reader5.Close();
                            }
                            else
                            {
                                MessageBox.Show("Boy ve kilo değerleri eksik.");
                            }

                            // 5. Cinsiyetten dolayı tüketemeyeceği ürünler ile karşılaştırma
                            string selectedGender = listBox2.Items.Cast<string>().FirstOrDefault(item => item == "Erkek" || item == "Kadın");

                            if (!string.IsNullOrEmpty(selectedGender))
                            {
                                SqlCommand command6 = new SqlCommand("SELECT * FROM cinsiyet WHERE Cinsiyet = @Cinsiyet", baglanti);
                                command6.Parameters.AddWithValue("@Cinsiyet", selectedGender);
                                SqlDataReader reader6 = command6.ExecuteReader();

                                if (reader6.Read())
                                {
                                    List<string> genderRestrictedItems = reader6["Tüketemeyeceği_ürünler"].ToString().Split(',').Select(i => i.Trim()).ToList();
                                    restrictedItems.AddRange(ingredients.Intersect(genderRestrictedItems));
                                }
                                reader6.Close();
                            }

                            // 6. Diğer Durumlardan dolayı tüketemeyeceği ürünler ile karşılaştırma
                            foreach (string condition in listBox2.Items)
                            {
                                // Check if the condition matches any dietary condition (e.g., Vegan, Vejeteryan, Hamile)
                                SqlCommand command6 = new SqlCommand("SELECT * FROM diger_durumlar WHERE Diğer_durumlar = @Diğer_durumlar", baglanti);
                                command6.Parameters.AddWithValue("@Diğer_durumlar", condition);
                                SqlDataReader reader6 = command6.ExecuteReader();

                                if (reader6.Read())
                                {
                                    List<string> conditionRestrictedItems = reader6["Tüketemeyeceği_ürünler"].ToString().Split(',').Select(i => i.Trim()).ToList();
                                    restrictedItems.AddRange(ingredients.Intersect(conditionRestrictedItems));
                                }
                                reader6.Close();
                            }

                            // Tüm yasaklı maddeler için tek bir mesaj oluştur
                            if (restrictedItems.Any())
                            {
                                var uniqueRestrictedItems = restrictedItems.Distinct().ToList();
                                string message = "Bu ürünü içerisindeki aşağıdaki maddelerden dolayı tüketemezsiniz: " + string.Join(", ", uniqueRestrictedItems);
                                MessageBox.Show(message);
                            }
                            else
                            {
                                MessageBox.Show("Bu ürün, tüketilebilir.");
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Veritabanı hatası: " + ex.Message);
                    }
                    finally
                    {
                        baglanti.Close();
                    }
                }
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
