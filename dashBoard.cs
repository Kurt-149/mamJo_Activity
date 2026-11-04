using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace activity3
{
    public partial class dashBoard : Form
    {
        public dashBoard() : this("Admin")
        {
        }

        public dashBoard(string username)
        {
            InitializeComponent();

            lblUser.Text = "Logged in: " + username;
            btnLogout.Click += btnLogout_Click;

            LoadProducts();
            LoadCategoryTotals();

            Shown += (s, e) =>
            {
                productGrid.ClearSelection();
                ActiveControl = null;
            };
        }

        private void LoadProducts()
        {
            try
            {
                DataTable table = Db.GetProducts();
                productGrid.Rows.Clear();

                int lowStock = 0;

                foreach (DataRow row in table.Rows)
                {
                    int qty = Convert.ToInt32(row["Qty"]);
                    if (qty < 10)
                    {
                        lowStock++;
                    }

                    productGrid.Rows.Add(
                        row["No"].ToString(),
                        row["Code"].ToString(),
                        row["Product"].ToString(),
                        row["Category"].ToString(),
                        qty.ToString(),
                        "\u20B1" + Convert.ToDecimal(row["Price"]).ToString("#,##0.00"),
                        "Edit   Delete");
                }

                lblTotal.Text = "Total items: " + table.Rows.Count;
                lblLow.Text = "Low stock: " + lowStock;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load products: " + ex.Message,
                    "Database error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadCategoryTotals()
        {
            try
            {
                Dictionary<string, int> totals = Db.GetCategoryTotals();

                lblWritingQty.Text = CategoryText(totals, "Writing");
                lblPaperQty.Text = CategoryText(totals, "Paper");
                lblArtQty.Text = CategoryText(totals, "Art");
                lblBagsQty.Text = CategoryText(totals, "Bags");
                lblToolsQty.Text = CategoryText(totals, "Tools");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load categories: " + ex.Message,
                    "Database error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string CategoryText(Dictionary<string, int> totals, string category)
        {
            int qty;
            if (!totals.TryGetValue(category, out qty))
            {
                return "0";
            }

            return qty < 10 ? qty + " low" : qty.ToString();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show(
                "Are you sure you want to log out?",
                "Log Out",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (answer == DialogResult.Yes)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}