using ClosedXML.Excel;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;

namespace CC2
{
    public partial class Index : Page
    {
        private string connectionString = "Server=MUQADDAS\\MSSQLSERVER01;Database=PlegitDB;Integrated Security=True;";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
               
                LoadCharityChampions("");
                CheckReferralCode();
            }
        }
        private void LoadCharityChampions(string searchKeyword)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT FirstName, LastName, ReferralCode, Commission, NoOfInstances, DonationsThisMonth, DonationsLastMonth, DonationsThisYear FROM CharityChampions";

                    if (!string.IsNullOrEmpty(searchKeyword))
                    {
                        query += $" WHERE FirstName LIKE '%{searchKeyword}%' OR LastName LIKE '%{searchKeyword}%' OR ReferralCode LIKE '%{searchKeyword}%'";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    // Get the host name of the current URL
                    string host = Request.Url.Host;
                    // Assuming you're using HTTPS, you can also get the scheme
                    string scheme = Request.Url.Scheme;

                    // Add RegistrationLink column
                    dt.Columns.Add("RegistrationLink", typeof(string));

                    // Generate RegistrationLink
                    foreach (DataRow row in dt.Rows)
                    {
                        string referralCode = row["ReferralCode"].ToString();
                        // Construct the registration link using the scheme, host, and referral code
                        row["RegistrationLink"] = $"{scheme}://{host}/?r={referralCode}";
                    }

                    charityChampionsGridView.DataSource = dt;
                    charityChampionsGridView.DataBind();
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error in LoadCharityChampions: " + ex.Message);
            }
        }

        private void CheckReferralCode()
        {
            try
            {
                string referralCode = Request.QueryString["r"];
                if (!string.IsNullOrEmpty(referralCode))
                {
                    // Database connection string

                    // Check if referral code exists in the database
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "SELECT COUNT(*) FROM CharityChampions WHERE ReferralCode = @ReferralCode";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@ReferralCode", referralCode);
                        int count = (int)cmd.ExecuteScalar();

                        if (count > 0)
                        {
                            // Increment the NoOfInstances column for the referral code
                            string updateQuery = "UPDATE CharityChampions SET NoOfInstances = NoOfInstances + 1 WHERE ReferralCode = @ReferralCode";
                            SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                            updateCmd.Parameters.AddWithValue("@ReferralCode", referralCode);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in Checking Referal Code" + ex.Message);
            }
        }

        protected void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime fromDateValue;
                DateTime toDateValue;

                string query;

                // Check if both from and to dates are selected
                if (DateTime.TryParse(fromDate.Text, out fromDateValue) && DateTime.TryParse(toDate.Text, out toDateValue))
                {
                    query = @"SELECT 
                     c.FirstName AS [First Name],
                     c.LastName AS [Last Name],
                     c.ReferralCode AS [Referral Code],
                     c.Commission AS [% Commission],
                     c.NoOfInstances AS [No. of Instances],
                     c.DonationsThisMonth AS [Donations This Month],
                     c.DonationsLastMonth AS [Donations Last Month],
                     c.DonationsThisYear AS [Donations This Year],
                     c.DateCreated AS [Date Created],
                     c.TimeCreated AS [Time Created] -- Added TimeCreated column
                 FROM CharityChampions c
                 WHERE c.DateCreated BETWEEN @FromDate AND @ToDate";
                }
                else
                {
                    // Select all records if from and to dates are not selected
                    query = @"SELECT 
                     c.FirstName AS [First Name],
                     c.LastName AS [Last Name],
                     c.ReferralCode AS [Referral Code],
                     c.Commission AS [% Commission],
                     c.NoOfInstances AS [No. of Instances],
                     c.DonationsThisMonth AS [Donations This Month],
                     c.DonationsLastMonth AS [Donations Last Month],
                     c.DonationsThisYear AS [Donations This Year],
                     c.DateCreated AS [Date Created],
                     c.TimeCreated AS [Time Created] -- Added TimeCreated column
                 FROM CharityChampions c";
                }


                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);

                    // Add parameters if from and to dates are selected
                    if (DateTime.TryParse(fromDate.Text, out fromDateValue) && DateTime.TryParse(toDate.Text, out toDateValue))
                    {
                        cmd.Parameters.AddWithValue("@FromDate", fromDateValue);
                        cmd.Parameters.AddWithValue("@ToDate", toDateValue);
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt, "CharityChampions");

                        Response.Clear();
                        Response.Buffer = true;
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition", "attachment;filename=CharityChampions.xlsx");

                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            wb.SaveAs(MyMemoryStream);
                            MyMemoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }
            catch (Exception ex) { 
            Console.WriteLine("error while generating excel file"+ex.Message);
            }
        }




        protected void AddCharityChampionButton_Click(object sender, EventArgs e)
        {
            // Code to handle adding a new charity champion
            // Show the modal
            ClientScript.RegisterStartupScript(this.GetType(), "ShowModal", "$(document).ready(function() { $('#addCharityChampionModal').modal('show'); });", true);
        }

        protected void SaveCharityChampionButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Retrieve values from input fields
                string firstName = firstNameTextBox.Text;
                string lastName = lastNameTextBox.Text;
                string commission = commissionTextBox.Text;

                // Generate a random 6 character alphanumeric referral code
                string referralCode = GenerateReferralCode();

                // Get current date and time
                DateTime currentDate = DateTime.Now;

                // Database connection string

                // Insert data into the database
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO CharityChampions (FirstName, LastName, ReferralCode, Commission, DateCreated, TimeCreated) VALUES (@FirstName, @LastName, @ReferralCode, @Commission, @DateCreated, @TimeCreated)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@ReferralCode", referralCode);
                    cmd.Parameters.AddWithValue("@Commission", commission);
                    cmd.Parameters.AddWithValue("@DateCreated", currentDate.ToString("dd/MM/yyyy")); // Store date in UK format
                    cmd.Parameters.AddWithValue("@TimeCreated", currentDate.TimeOfDay); // Store only the time component

                    cmd.ExecuteNonQuery();
                }

                // Refresh the page or update the UI as needed
                LoadCharityChampions("");
            }
            catch (Exception ex) {
                Console.WriteLine("Error while adding champion" + ex.Message);
            }
        }


        private string GenerateReferralCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            char[] code = new char[6];
            string referralCode;

            // Keep generating codes until a unique one is found
            do
            {
                for (int i = 0; i < code.Length; i++)
                {
                    code[i] = chars[random.Next(chars.Length)];
                }
                referralCode = new string(code);
            }
            while (!IsReferralCodeUnique(referralCode));

            return referralCode;
        }

        private bool IsReferralCodeUnique(string referralCode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM CharityChampions WHERE ReferralCode = @ReferralCode";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ReferralCode", referralCode);
                    int count = (int)cmd.ExecuteScalar();

                    // If count is 0, the referral code is unique
                    return count == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in IsReferralCodeUnique: " + ex.Message);
                // Return false to indicate that uniqueness check failed due to an error
                return false;
            }
        }


    }
}
