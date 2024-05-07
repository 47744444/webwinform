using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net.Http;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using System.Configuration; // 引用 System.Configuration 命名空間

namespace WindowsFormsApp14
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //初始化資料表
        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: 這行程式碼會將資料載入 'fixDataSet.Solution' 資料表。您可以視需要進行移動或移除。
            this.solutionTableAdapter.Fill(this.fixDataSet.Solution);
            // TODO: 這行程式碼會將資料載入 'fixDataSet.Status' 資料表。您可以視需要進行移動或移除。
            this.statusTableAdapter.Fill(this.fixDataSet.Status);
            // TODO: 這行程式碼會將資料載入 'fixDataSet.Device' 資料表。您可以視需要進行移動或移除。
            this.deviceTableAdapter.Fill(this.fixDataSet.Device);
            // TODO: 這行程式碼會將資料載入 'fixDataSet.FixReason' 資料表。您可以視需要進行移動或移除。
            this.fixReasonTableAdapter.Fill(this.fixDataSet.FixReason);
            // TODO: 這行程式碼會將資料載入 'fixDataSet.Location' 資料表。您可以視需要進行移動或移除。
            this.locationTableAdapter.Fill(this.fixDataSet.Location);
            // TODO: 這行程式碼會將資料載入 'fixDataSet.v_Device' 資料表。您可以視需要進行移動或移除。
            this.v_DeviceTableAdapter.Fill(this.fixDataSet.v_Device);
            comboBox1.Text = "必填";
            comboBox2.Text = "必填";
            comboBox3.Text = "必填";



        }

        string connectionString = ConfigurationManager.ConnectionStrings["WindowsFormsApp14.Properties.Settings.FixConnectionString"].ConnectionString;

        //儲存編輯功能
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 建立與資料庫的連線
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 取得目前選中的整列資料的索引
                    int rowIndex = dataGridView1.CurrentCell.RowIndex;

                    // 如果沒有選中任何資料列，則返回
                    if (rowIndex < 0)
                    {
                        MessageBox.Show("請選擇要更新的資料", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 取得目前選中的整列資料
                    DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];

                    // 執行更新資料的 SQL 命令
                    string sql = @"UPDATE Device 
                           SET EmpID = @EmpID, 
                               FixEmpID = @FixEmpID, 
                               Remark = @Remark,
                               Location = @Location,
                               FixReason = @FixReason,
                               Status = @Status,
                               Solution = @Solution

                           WHERE ID = @ID";
                    int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                    byte location = Convert.ToByte(comboBox1.SelectedValue);
                    byte fixreason = Convert.ToByte(comboBox2.SelectedValue);
                    byte status = Convert.ToByte(comboBox3.SelectedValue);
                    byte solution = Convert.ToByte(comboBox4.SelectedValue);
                    string message = "\n" +
                             "報修原因: " + comboBox2.Text + "\n" +
                             "設備位置: " + comboBox1.Text + "\n" +
                             "備註: " + textBox4.Text + "\n" +
                             "報修工號: " + textBox2.Text + "\n" +
                             "維修進度: " + comboBox3.Text + "\n" +
                             "維修工號: " + textBox3.Text + "\n" +
                             "解決方式: " + comboBox4.Text + "\n" +
                             "推薦解決方式: " + GetRecommendedSolution(fixreason);
                    SaveMessageToNetworkFolder(message);
                    // 創建一個匿名物件來傳遞參數
                    var parameters = new
                    {

                        ID = id,
                        EmpID = textBox2.Text,
                        FixEmpID = textBox3.Text,
                        Remark = textBox4.Text,
                        Location = location,
                        FixReason = fixreason,
                        Status = status,
                        Solution = solution
                    };

                    // 執行 SQL 命令
                    connection.Execute(sql, parameters);

                    MessageBox.Show("資料更新成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //SendLineNotify(message);

                }
                // 插入資料後重新載入或重新綁定資料表
                Form1_Load(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("發生錯誤: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void fillByToolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                this.v_DeviceTableAdapter.FillBy(this.fixDataSet.v_Device);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }
        // 點擊資料列將資料帶入上方編輯區
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // 取得使用者所選的整列資料
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];

                // 從選中的整列資料中取得需要的資料，並將其填入 TextBox 中
                textBox1.Text = selectedRow.Cells["Id"].Value.ToString();
                textBox2.Text = selectedRow.Cells["EmpID"].Value.ToString();
                textBox3.Text = selectedRow.Cells["FixEmpID"].Value.ToString();
                textBox4.Text = selectedRow.Cells["Remark"].Value.ToString();
                comboBox1.Text = selectedRow.Cells["Location"].Value.ToString();
                comboBox2.Text = selectedRow.Cells["FixReason"].Value.ToString();
                comboBox3.Text = selectedRow.Cells["Status"].Value.ToString();
                comboBox4.Text = selectedRow.Cells["Solution"].Value.ToString();
                // 依此類推，將你需要的資料填入到相應的 TextBox 中
            }
        }
        //新增項目
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // 建立與資料庫的連線
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 執行插入資料的 SQL 命令
                    string sql = @"INSERT INTO Device (EmpID, FixEmpID, Remark, Location, FixReason, Status, Solution,LineNotify)
                           VALUES (@EmpID, @FixEmpID, @Remark, @Location, @FixReason, @Status, @Solution,@LineNotify)";

                    // 獲取要插入的值

                    byte location = Convert.ToByte(comboBox1.SelectedValue);
                    byte fixreason = Convert.ToByte(comboBox2.SelectedValue);
                    byte status = Convert.ToByte(comboBox3.SelectedValue);
                    byte solution = Convert.ToByte(comboBox4.SelectedValue);
                    byte linenotify = 0; 
                    GetRecommendedSolution(fixreason);
                    string message = "\n" +
                                     "報修原因: " + comboBox2.Text + "\n" +
                                     "設備位置: " + comboBox1.Text + "\n" +
                                     "備註: " + textBox4.Text + "\n" +
                                     "報修工號: " + textBox2.Text + "\n" +
                                     "維修進度: " + comboBox3.Text + "\n" +
                                     "維修工號: " + textBox3.Text + "\n" +
                                     "解決方式: " + comboBox4.Text + "\n" +
                                     "推薦解決方式: " + GetRecommendedSolution(fixreason);
                    SaveMessageToNetworkFolder(message);
                    // 創建參數對象
                    var parameters = new
                    {
                        EmpID = textBox2.Text,
                        FixEmpID = textBox3.Text,
                        Remark = textBox4.Text,
                        Location = location,
                        FixReason = fixreason,
                        Status = status,
                        Solution = solution,
                        LineNotify = linenotify
                    };

                    // 執行 SQL 命令
                    connection.Execute(sql, parameters);

                    // 提示資料插入成功
                    MessageBox.Show("資料插入成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //SendLineNotify(message);
                }

                // 插入資料後重新載入或重新綁定資料表
                Form1_Load(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("發生錯誤: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetRecommendedSolution(byte fixreason)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT TOP 3 s.SolutionDesc
                        FROM (
                            SELECT Solution, ROW_NUMBER() OVER (ORDER BY SolutionCount DESC) AS RowNum
                            FROM (
                                SELECT Solution, COUNT(*) AS SolutionCount
                                FROM device
                                WHERE FixReason = @FixReason
                                GROUP BY Solution
                            ) AS SolutionCounts
                        ) AS RankedSolutions
                        JOIN Solution s ON RankedSolutions.Solution = s.SolutionId
                        ORDER BY RankedSolutions.RowNum;";
                var parameters = new
                {
                    FixReason = fixreason
                };
                var solutions = connection.Query<string>(sql, parameters);

                if (solutions != null && solutions.Any())
                {
                    var solutionStringBuilder = new StringBuilder();
                    foreach (var solution in solutions)
                    {
                        solutionStringBuilder.Append(solution + ", ");
                    }
                    // 移除最後多餘的逗號和空格
                    solutionStringBuilder.Length -= 2;
                    return solutionStringBuilder.ToString();
                }
                else
                {
                    return ""; // 沒有找到解決方案
                }
            }
        }
        private void SaveMessageToNetworkFolder(string message)
        {
            try
            {
                // 設定網路共用資料夾的路徑
                string networkFolderPath = @"share(\\192.168.168.120)\YL_Tool\Fix_txt"; // 請將 ServerName 和 SharedFolder 替換為實際的共用資料夾路徑

                // 檢查網路共用資料夾是否存在
                if (Directory.Exists(networkFolderPath))
                {
                    // 建立檔案路徑
                    string filePath = Path.Combine(networkFolderPath, "message.txt");

                    // 將訊息寫入檔案
                    File.WriteAllText(filePath, message, Encoding.UTF8);

                    MessageBox.Show("訊息已成功儲存到網路共用資料夾中", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("找不到指定的網路共用資料夾", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("儲存訊息到網路共用資料夾時發生錯誤: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        private void SendLineNotify(string message)
        {
            // Replace 'YOUR_LINE_NOTIFY_API_TOKEN' with your actual Line Notify API token
            string lineNotifyApiToken = "bY3JQtEL5N06GKqpIIG5vmMPWR205EmBgfumGQYBYv6";
            string lineNotifyApiUrl = "https://notify-api.line.me/api/notify";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + lineNotifyApiToken);
                var content = new StringContent("message=" + message, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = httpClient.PostAsync(lineNotifyApiUrl, content).Result;
                // You can handle response here if needed
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // 建立與資料庫的連線
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 取得目前選中的整列資料的索引
                    int rowIndex = dataGridView1.CurrentCell.RowIndex;

                    // 如果沒有選中任何資料列，則返回
                    if (rowIndex < 0)
                    {
                        MessageBox.Show("請選擇要更新的資料", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 取得目前選中的整列資料
                    DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];

                    // 執行更新資料的 SQL 命令
                    string sql = @"UPDATE Device 
                           SET EmpID = @EmpID, 
                               FixEmpID = @FixEmpID, 
                               Remark = @Remark,
                               Location = @Location,
                               FixReason = @FixReason,
                               Status = @Status,
                               Solution = @Solution,
                               LineNotify=@LineNotify

                           WHERE ID = @ID";
                    int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                    byte location = Convert.ToByte(comboBox1.SelectedValue);
                    byte fixreason = Convert.ToByte(comboBox2.SelectedValue);
                    byte status = Convert.ToByte(comboBox3.SelectedValue);
                    byte solution = Convert.ToByte(comboBox4.SelectedValue);
                    byte linenotify = 1;
                    // 創建一個匿名物件來傳遞參數
                    var parameters = new
                    {

                        ID = id,
                        EmpID = textBox2.Text,
                        FixEmpID = textBox3.Text,
                        Remark = textBox4.Text,
                        Location = location,
                        FixReason = fixreason,
                        Status = status,
                        Solution = solution,
                        LineNotify = linenotify
                    };

                    // 執行 SQL 命令
                    connection.Execute(sql, parameters);

                    MessageBox.Show("資料更新成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //SendLineNotify(message);

                }
                // 插入資料後重新載入或重新綁定資料表
                Form1_Load(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("發生錯誤: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

