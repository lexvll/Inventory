using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Inventory
{
    public partial class Register : Form
    {
        public Register()
            {
                InitializeComponent();

                btnRegister.Click += btnRegister_Click;
                txtComfirmPassword.UseSystemPasswordChar = true;

                linkSignIn.LinkClicked += linkSignIn_LinkClicked;
            }

        private void linkSignIn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Register_Load(object sender, EventArgs e)
        {

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string fullname = txtFullname.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string comfirmPassword = txtComfirmPassword.Text;

            if (fullname == "" || username == "" || password == "" || comfirmPassword == "")
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (password != comfirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username= ?";

                    using (OleDbCommand checkCmd = new OleDbCommand(checkUserQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@username", username);
                        int userCount = (int)checkCmd.ExecuteScalar();
                        if (userCount > 0)
                        {
                            MessageBox.Show("Username already exists.Please use different username.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    string query = @"INSERT INTO [Users] ([FullName], [Username], [Password]) VALUES (?, ?, ?)";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        string passwordHash = PasswordHelper.HashPassword(password);
                        cmd.Parameters.AddWithValue("@fullname", fullname);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", passwordHash);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Registration successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            txtFullname.Clear();
                            txtUsername.Clear();
                            txtPassword.Clear();
                            txtComfirmPassword.Clear();

                            SignIn loginForm = new SignIn();
                            loginForm.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Registration failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while registering: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            SignIn login = new SignIn();
            login.Show();
            this.Hide();
        }
    }
}
