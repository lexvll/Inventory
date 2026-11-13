using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Runtime.Serialization;

namespace Inventory
{
    public partial class Dashboard : Form
    {
        private int selectedProductID = 0;
        public Dashboard()
        {
            InitializeComponent();

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += btnClear_Click;
            btnRefresh.Click += btnRefresh_Click;

            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;

            txtSearch.TextChanged += txtSearch_TextChange;
            dgvProducts.CellClick += dgvProducts_CellClick;

            LoadProducts();
            LoadDashboardStats();
        }

        private void LoadDashboardStats()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string quesry = @"SELECT COUNT(*) AS TotalProducts, SUM(IIF(Quality > ReorderLevel, 1 ,0)) AS InStock, SUM(IIF(Quality > 0 AND Quality <= ReorderLevel, 1, 0)) AS LowStock, SUM(IIF(Quality = 0, 1, 0)) AS OutOfStock FROM Products";

                    using (OleDbCommand cmd = new OleDbCommand(quesry, con))
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblTotalProduct.Text = reader["TotalProducts"].ToString();
                            lblInStock.Text = reader["InStock"].ToString(); 
                            lblLowStock.Text = reader["LowStock"].ToString();
                            lblOutOfStock.Text = reader["OutOfStock"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Statistic error:" + ex.Message);
            }
        }

        private void LoadProducts(string search = "")
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"SELECT * FROM Products";

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        query += @" WHERE ProductName LIKE ? OR Category LIKE ? OR Description LIKE ?";
                    }

                    query += @" ORDER BY ProductID DESC";

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, con))
                    {
                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            string pattern = "%" + search + "%";
                            adapter.SelectCommand.Parameters.Add("?", OleDbType.VarWChar).Value = pattern;
                            adapter.SelectCommand.Parameters.Add("?", OleDbType.VarWChar).Value = pattern;
                            adapter.SelectCommand.Parameters.Add("?", OleDbType.VarWChar).Value = pattern;
                        }

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvProducts.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load products: " + ex.Message);
            }
        }

        private bool ValiddateProduct()
        {
            if (string.IsNullOrEmpty(txtProductName.Text))
            {
                MessageBox.Show("Please enter product name.");
                txtProductName.Focus();
                return false;
            }

            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please select product category.");
                cmbCategory.Focus();
                return false;
            }

            decimal price;

            if (!decimal.TryParse(txtUnitPrice.Text, out price) || price <0)
            {
                MessageBox.Show("Please enter product price");
                txtUnitPrice.Focus();
                return false;
            }

            int quality;

            if (!int.TryParse(txtQuality.Text, out quality) || quality < 0)
            {
                MessageBox.Show("Please enter product quality.");
                txtProductName.Focus();
                return false;
            }

            int reorder;

            if ((!int.TryParse(txtReorderLevel.Text, out reorder) || reorder < 0))
            {
                MessageBox.Show("Please enter a valid reorder level.");
                txtReorderLevel.Focus();
                return false;
            }
            
            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if(!ValiddateProduct())
                return;

            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"INSERT INTO Products (ProductName, Category, Description, UnitPrice, Quality, ReorderLevel) VALUES (?,?,?,?,?,?)";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtProductName.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = cmbCategory.Text;
                        cmd.Parameters.Add("?", OleDbType.LongVarWChar).Value = txtDescription.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.Currency).Value = decimal.Parse(txtUnitPrice.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtQuality.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtReorderLevel.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Product added successfully!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearField();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding product:" + ex.Message);
            }

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if(selectedProductID == 0)
            {
                MessageBox.Show("Please select a product row to update.");
                return;
            }

            if (!ValiddateProduct())
                return;

            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"UPDATE Products SET ProductName=?, Category=?, Description=?, UnitPrice=?, Quality=?, ReorderLevel=? WHERE ProductID=?";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtProductName.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = cmbCategory.Text;
                        cmd.Parameters.Add("?", OleDbType.LongVarWChar).Value = txtDescription.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.Currency).Value = decimal.Parse(txtUnitPrice.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtQuality.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtReorderLevel.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedProductID;

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Product updated successfully!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearField();
                LoadDashboardStats();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating product:" + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product row to delete.");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this product?", "Confirm Deelte", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if(result == DialogResult.Yes)
            {
                try
                {
                    using (OleDbConnection con = Database.GetConnection())
                    {
                        con.Open();

                        string query = @"DELETE FROM Products WHERE ProductID = ?";

                        using (OleDbCommand cmd = new OleDbCommand(query, con))
                        {
                       
                            cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedProductID;

                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Product deleted successfully!");
                    ClearField();
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting product:" + ex.Message);
                }
            }
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

                selectedProductID = Convert.ToInt32(row.Cells["ProductID"].Value);

                txtProductName.Text = Convert.ToString(row.Cells["ProductName"].Value);
                cmbCategory.Text = Convert.ToString(row.Cells["Category"].Value);
                txtDescription.Text = Convert.ToString(row.Cells["Description"].Value);
                txtUnitPrice.Text = Convert.ToString(row.Cells["UnitPrice"].Value);
                txtQuality.Text = Convert.ToString(row.Cells["Quality"].Value);
                txtReorderLevel.Text = Convert.ToString(row.Cells["ReorderLevel"].Value);   
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {   
            ClearField();
        }

        private void txtSearch_TextChange(object sender, EventArgs e)
        {
            LoadProducts(txtSearch.Text.Trim());
        }
        private void ClearField()
        {
            txtProductName.Clear();
            cmbCategory.SelectedIndex = -1;
            txtDescription.Clear();
            txtUnitPrice.Clear();
            txtQuality.Clear();
            txtReorderLevel.Clear();

            selectedProductID = 0;

            dgvProducts.ClearSelection();
        }



        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                    {
                        Application.Restart();

                    }
                }
            

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
           
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void labelCategory_Click(object sender, EventArgs e)
        {

        }

        private void txtcategory_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            ClearField();
            LoadProducts();
            LoadDashboardStats();
        }
    }
}
