﻿using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Proiect_final_MTP
{
    public partial class FoaieMatricola : UserControl
    {
        static string fileName;
        static string sourcePath;
        static string destinationPath;
        MySqlConnection sqlConnection = Connection.getSqlConnection();

        public FoaieMatricola()
        {
            InitializeComponent();
        }

        #region Upload Files
        private void FoaieMatricola_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }


        private void FoaieMatricola_DragDrop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in droppedFiles)
            {
                this.timer.Start();
                
                pcbUpload.Visible = false;
                lblDragDrop.Visible = false;
                progressBarUpload.Visible = true;
                FoaieMatricola.fileName = getFileName(file);
                FoaieMatricola.sourcePath = getFilePath(file);
            }
        }


        // salvare fisiere incarcate
        private void btnSalvareModificari_Click(object sender, EventArgs e)
        {
            string sourcePath, destinationPath;
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                FoaieMatricola.destinationPath = folderBrowserDialog.SelectedPath;
            }

            try
            {
                for (int i = 0; i < lbxFileName.Items.Count; i++)
                {
                    destinationPath = FoaieMatricola.destinationPath + "\\" + lbxFileName.Items[i].ToString();
                    sourcePath = FoaieMatricola.sourcePath;
                    destinationPath = Path.Combine(sourcePath, destinationPath);
                    File.Copy(sourcePath, destinationPath, true);
                }

                if (lbxFileName.Items.Count == 1)
                {
                    MessageBox.Show("Incarcare fisier cu succes!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    defaultListBox();
                }
                else if (lbxFileName.Items.Count > 1)
                {
                    MessageBox.Show("Incarcare fisiere cu succes!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    defaultListBox();
                }
                else
                    MessageBox.Show("Nu au fost incarcate fisiere!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);


            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void timer_Tick(object sender, EventArgs e)
        {
            this.progressBarUpload.Increment(7);

            if(progressBarUpload.Value >= progressBarUpload.Maximum)
            {
                timer.Stop();
                progressBarUpload.Value = progressBarUpload.Minimum;
                progressBarUpload.Visible = false;
                lbxFileName.Items.Add(FoaieMatricola.fileName);
            }
        }


        // metoda ce returneaza calea fisierului 
        private string getFilePath(string file)
        {
            return Path.GetFullPath(file);
        }
        

        // metoda ce returneaza numele fisierului
        private string getFileName(string file)
        {
            return Path.GetFileName(file);
        }


        // buton cancel
        private void btnCancel_Click(object sender, EventArgs e)
        {
            defaultListBox();
        }


        // resetare listBox
        private void defaultListBox()
        {
            pcbUpload.Visible = true;
            lblDragDrop.Visible = true;
            progressBarUpload.Visible = false;
            lbxFileName.Items.Clear();
        }
        #endregion


        #region generare Foaie Matricola
        private void btnGenerare_Click(object sender, EventArgs e)
        {
            string queryMediiDiscipline =
                " SELECT disciplina," +
                "        an_studiu," +
                "        MAX(nota) AS media" +
                " FROM note" +
                " WHERE note.nr_legitimatie = '" + Student.Legitimatie + "'" +
                " GROUP BY disciplina," +
                "          an_studiu" +
                " ORDER BY an_studiu DESC," +
                "           disciplina";


            string queryMediiAnuale =
                " SELECT medii.medie_an1," +
                "        medii.medie_an2," +
                "        medii.medie_an3," +
                "        medii.medie_generala" +
                " FROM medii" +
                " WHERE nr_legitimatie = '" + Student.Legitimatie + "'";

            DataTable dtMediiDiscipline = makeDataTable(queryMediiDiscipline);
            DataTable dtMediiAnuale = makeDataTable(queryMediiAnuale);


            exportFoaieMatricola(dtMediiDiscipline, dtMediiAnuale, "Foaie_matricola_" + Student.Nume + "_" + Student.Prenume);

        }


        // metoda prin care se exporta datele studentului intr-un document PDF
        private void exportFoaieMatricola(DataTable dtMediiDiscipline, DataTable dtMediiAnuale, String fileName)
        {
            PdfWriter pdfWriter = new PdfWriter(fileName);
            PdfDocument pdfDocument = new PdfDocument(pdfWriter);
            Document document = new Document(pdfDocument);
            Style style = new Style();

            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);


            #region header
            Paragraph headerParagraph = new Paragraph()
                .Add("Foaie matricola".ToUpper())
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16)
                .SetFont(font)
                .SetFontColor(ColorConstants.DARK_GRAY);
            #endregion


            #region data generarii documentului
            Paragraph dataParagraph = new Paragraph()
                .Add("\n" + DateTime.Now.ToShortDateString())
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(12)
                .SetFont(font)
                .SetFontColor(ColorConstants.DARK_GRAY);
            #endregion


            #region informatii student
            Paragraph infoParagraph = new Paragraph()
                .Add("Studentul: " + Student.FullName().ToUpper())
                .Add("\nSpecializarea: ".ToUpper() + "Informatica")
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFontSize(12)
                .SetFont(font)
                .SetFontColor(ColorConstants.DARK_GRAY);
            #endregion


            #region linie separatoare
            LineSeparator lineSeparator = new LineSeparator(new SolidLine());
            #endregion


            #region linie noua
            Paragraph newLineParagraph = new Paragraph(new Text("\n"));
            #endregion


            #region tabel medii discipline
            Table tableMediiDiscipline = new Table(dtMediiDiscipline.Columns.Count, false);
            tableMediiDiscipline.SetMinWidth(100)
                 .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER)
                 .SetFontSize(12);

            for (int i = 0; i < dtMediiDiscipline.Columns.Count; i++)
            {
                Cell cell = new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY)
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.WHITE)
                    .Add(new Paragraph(dtMediiDiscipline.Columns[i].ColumnName.ToUpper()));

                tableMediiDiscipline.AddCell(cell);
            }

            for (int i = 0; i < dtMediiDiscipline.Rows.Count; i++)
            {
                for (int j = 0; j < dtMediiDiscipline.Columns.Count; j++)
                {
                    tableMediiDiscipline.AddCell(dtMediiDiscipline.Rows[i][j].ToString());
                }
            }
            #endregion


            #region tabel medii anuale
            Table tableMediiAnuale = new Table(dtMediiAnuale.Columns.Count, false);
            tableMediiAnuale.SetMinWidth(100)
                 .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER)
                 .SetFontSize(12);

            for (int i = 0; i < dtMediiAnuale.Columns.Count; i++)
            {
                Cell cell = new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY)
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.WHITE)
                    .Add(new Paragraph(dtMediiAnuale.Columns[i].ColumnName.ToUpper()));

                tableMediiAnuale.AddCell(cell);
            }

            for (int i = 0; i < dtMediiAnuale.Rows.Count; i++)
            {
                for (int j = 0; j < dtMediiAnuale.Columns.Count; j++)
                {
                    tableMediiAnuale.AddCell(dtMediiAnuale.Rows[i][j].ToString());
                }
            }
            #endregion


            #region semnatura
            Paragraph footerParagraph = new Paragraph()
                .Add("Student")
                .Add("\n" + Student.FullName().ToUpper())
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(12)
                .SetFont(font)
                .SetFontColor(ColorConstants.DARK_GRAY);
            #endregion


            #region generare PDF
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = fileName;
            saveFileDialog.DefaultExt = ".pdf";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                document.Add(headerParagraph);
                document.Add(dataParagraph);
                document.Add(infoParagraph);
                document.Add(lineSeparator);
                document.Add(newLineParagraph);
                document.Add(newLineParagraph);
                document.Add(tableMediiDiscipline);
                document.Add(newLineParagraph);
                document.Add(newLineParagraph);
                document.Add(tableMediiAnuale);
                document.Add(newLineParagraph);
                document.Add(newLineParagraph);
                document.Add(newLineParagraph);
                document.Add(newLineParagraph);
                document.Add(footerParagraph);
                document.Close();

                MessageBox.Show("Document generat cu succes!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblDataFoaieM.Visible = true;
                lblDataFoaieM.Text = "Data generare: " + DateTime.Now.ToString();


                Process.Start(fileName);
            }
            #endregion
        }


        // creare DataTable cu datele din BD
        private DataTable makeDataTable(string query)
        {
            sqlConnection.Open();

            MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
            dataAdapter.SelectCommand = new MySqlCommand(query, sqlConnection);

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            dataTable.Dispose();
            dataAdapter.Dispose();
            sqlConnection.Close();


            return dataTable;
        }
        #endregion

    }
}
