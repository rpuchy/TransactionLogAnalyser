using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ReadWriteCsv;

namespace TransactionLog_Analyser
{

 


    public partial class Form1 : Form
    {
        public enum column_headers  { StateChange=0, TransactionType, TransactionSubType, Action, Portfolio, TaxWrapper, Product, Asset_Liability, StateCounter, TransactionCounter, Scenario, TimeStep, Sell, Buy, Price, UseSellPriceToBuy, Holdings, SellValue };


        public List<Transaction> Transactions = new List<Transaction>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "";
            openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            processFile(textBox1.Text);
        }

        private void processFile(string fileName)
        {
            Transactions.Clear();
            using (var fs = File.OpenRead(fileName))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    Transaction t = new Transaction() {
                        StateChange =values[(int)column_headers.StateChange],
                        TransactionType = values[(int)column_headers.TransactionType],
                        priority = values[(int)column_headers.TransactionSubType]!="" ? long.Parse(values[(int)column_headers.TransactionSubType]):0,
                        Action = values[(int)column_headers.Action],
                        Portfolio = values[(int)column_headers.Portfolio],
                        TaxWrapper =  values[(int)column_headers.TaxWrapper],
                        Product = values[(int)column_headers.Product],
                        Asset_Liability = values[(int)column_headers.Asset_Liability],
                        StateCounter = values[(int)column_headers.StateCounter],
                        TransactionCounter = values[(int)column_headers.TransactionCounter],
                        Scenario = long.Parse(values[(int)column_headers.Scenario]),
                        TimeStep = long.Parse(values[(int)column_headers.TimeStep]),
                        Sell = values[(int)column_headers.Sell]!=""? double.Parse(values[(int)column_headers.Sell]):0.0,
                        Buy = values[(int)column_headers.Buy]!=""?double.Parse(values[(int)column_headers.Buy]):0.0,
                        Price = double.Parse(values[(int)column_headers.Price]),
                        UseSellPriceToBuy = values[(int)column_headers.UseSellPriceToBuy].Equals("1"),
                        Holdings = double.Parse(values[(int)column_headers.Holdings]),
                        SellValue = double.Parse(values[(int)column_headers.SellValue])
                    };

                    Transactions.Add(t);
                }
            }

            List<string> products = new List<string>();
            List<long> timesteps = new List<long>();
            List<long> trials = new List<long>();
            foreach(var transaction in Transactions)
            {
                products.Add(transaction.Portfolio + ',' + transaction.TaxWrapper + ',' + transaction.Product);
                timesteps.Add(transaction.TimeStep);
                trials.Add(transaction.Scenario);

            }
            productlist.Items.Clear();
            foreach (var prod in products.Distinct().ToList())
            {
                productlist.Items.Add(prod);
            }
            timestepStart.Items.Clear();
            scenario.Items.Clear();
            foreach(var ts in timesteps.Distinct().ToList())
            {
                timestepStart.Items.Add(ts);
            }
            timestepStart.SelectedIndex = Math.Min(2,timestepStart.Items.Count-1);
            foreach(var sc in trials.Distinct().ToList())
            {
                scenario.Items.Add(sc);
            }
            scenario.SelectedIndex = 0;
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("Timestep","Timestep");
            dataGridView1.Columns.Add("TransactionName", "TransactionName");
            dataGridView1.Columns.Add("Amount", "Amount");
            dataGridView1.Columns.Add("ValueBefore", "ValueBefore");
            dataGridView1.Columns[1].Width =300;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //What should happen here is we go to the results file, get the timestep start value then show all the transactions that changed a specific product
            //get product details from selection

        }

        private void productlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            for (int i = int.Parse(timestepStart.Text); i <=timesteps.Value; i++)
            {
                foreach (List<string> row in GetTimestepData(long.Parse(scenario.Text),i, productlist.Items[productlist.SelectedIndex].ToString()))
                {
                    dataGridView1.Rows.Add(row.ToArray<string>());
                }
            }
        }

        private List<List<string>> GetTimestepData(long Scenario, long timestep, string Prod)
        {
            var prod = Prod.Split(',');
            List<Transaction> sublist = Transactions.Where(x => x.TimeStep == timestep && x.Scenario == Scenario && x.Portfolio == prod[0] && x.TaxWrapper == prod[1] && x.Product == prod[2] && x.Action != "Execute").ToList();
            List<List<string>> temp = new List<List<string>>();
            foreach (Transaction t in sublist)
            {
                List<string> tList = new List<string>();
                tList.Add(timestep.ToString());
                tList.Add(t.TransactionType);
                tList.Add(String.Format("{0:n}", (t.Action == "Buy") ? t.Buy : t.Sell * (-1)));
                tList.Add(String.Format("{0:n}", t.SellValue));

                temp.Add(tList);
            }
            if (sublist.Count > 0)
            {
                List<string> tList = new List<string>();
                tList.Add(timestep.ToString());
                tList.Add("End of Timestep");
                tList.Add("0");
                tList.Add(String.Format("{0:n}", (double.Parse(temp[temp.Count - 1][3]) + double.Parse(temp[temp.Count - 1][2]))));
                temp.Add(tList);
            }
            return temp;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (CsvFileWriter writer = new CsvFileWriter(saveFileDialog1.FileName))
                {
                    CsvRow Header = new CsvRow();
                    Header.Add("Timestep");
                    Header.Add("TransactionName");
                    Header.Add("Amount");
                    Header.Add("ValueBefore");
                    writer.WriteRow(Header);
                    for (int i = 0; i < dataGridView1.Rows.Count-1;i++)
                    {
                        CsvRow row = new CsvRow();
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {                            
                            row.Add(dataGridView1[j, i].Value.ToString());
                        }
                        writer.WriteRow(row);
                    }
                }
            }


        }
    }
}
