﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web.Security;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;
using System.Text;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.SqlClient;
using com.eppi.utils;
using System.Net;
using System.ComponentModel;
using System.Runtime.InteropServices;



using FGWHSEClient.DAL;

namespace FGWHSEClient.Form
{
    public partial class HTaspDelivery_New2 : System.Web.UI.Page
    {
        DataTable dtScanned = new DataTable();
        Maintenance maintScanned = new Maintenance();

        ASPDAL aDAL = new ASPDAL();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtbxDSRefNo.Focus();
                addDetails();
                createDataTable(dtScanned);
                ViewState["dtScanned"] = dtScanned;
            }
            else
            {
                dtScanned = (DataTable)ViewState["dtScanned"];
            }


        }

        public void createDataTable(DataTable dt)
        {
            dt.Columns.Add("DSREFNO", typeof(string));
            dt.Columns.Add("SERIALNO", typeof(string));
            dt.Columns.Add("PARTCODE", typeof(string));
        }


        public void lnk_Click(object sender, EventArgs e)
        {
            var rowTrigger = (Control)sender;
            GridViewRow rowUpdate = (GridViewRow)rowTrigger.NamingContainer;
            int rowIndx = rowUpdate.RowIndex;
            LinkButton lnk = (LinkButton)gvDeliver.Rows[rowIndx].FindControl("lnkDS");
            deleteDatatableRow(dtScanned, gvDeliver, lnk.Text);
            MsgBox1.alert("DS " + lnk.Text + " has been deleted to the list!");


        }
        public void deleteDatatableRow(DataTable dt, GridView gv, string strDS)
        {

            DataRow[] drr = dt.Select("DSREFNO='" + strDS + "'");
            for (int i = 0; i < drr.Length; i++)
            {
                drr[i].Delete();
            }


            dt.AcceptChanges();
            ViewState["dtScanned"] = dt;
            bindGrid(dt, gv);
        }

        public bool CheckIfTextExistsInDataTable(DataTable dt, string strColumn, string strText)
        {
            bool exists = false;

            DataRow[] drr = dt.Select(strColumn + "='" + strText + "'");
            if (drr.Length > 0)
            {
                exists = true;
            }
            return exists;
        }

        public void AddValueDataTable(DataTable dt, DataTable dtValue)
        {
            DataRow dr;
            DataView dv = dtValue.DefaultView;
            if (dv.Count > 0)
            {
                for (int x = 0; x < dv.Count; x++)
                {
                    dr = dt.NewRow();
                    dr["DSREFNO"] = txtbxDSRefNo.Text;
                    dr["SERIALNO"] = dv[x]["SerialNo"].ToString();
                    dr["PARTCODE"] = dv[x]["Partcode"].ToString();
                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();
            }
        }
        private void bindGrid(DataTable dt, GridView Grd)
        {
            Grd.DataSource = null;
            Grd.DataSource = dt;
            Grd.DataBind();
        }
        public DataTable DataTableToViewStatActions(string strDS, DataSet ds)
        {
            DataTable dt = new DataTable();
            dt = ds.Tables[0];
            return dt;
        }

        public void addDetails()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("SerialNo", typeof(string));
            dt.Columns.Add("Partcode", typeof(string));
            ViewState["gvDeliver"] = dt;
            BindGrid();
        }

        protected void BindGrid()
        {
            gvDeliver.DataSource = ViewState["gvDeliver"] as DataTable;
            gvDeliver.DataBind();
        }

        protected void txtbxDSRefNo_TextChanged(object sender, EventArgs e)
        {
            DataTable dtValue = maintScanned.getDSRefNoDetails(txtbxDSRefNo.Text).Tables[0];
            if (CheckIfTextExistsInDataTable(dtScanned, "DSREFNO", txtbxDSRefNo.Text.Trim()) == false)
            {
                AddValueDataTable(dtScanned, dtValue);
            }
            else
            {
                MsgBox1.alert("DS Refno " + txtbxDSRefNo.Text + "already scanned!");
           
            }


            ViewState["dtScanned"] = dtScanned;
            if (dtValue.DefaultView.Count == 0)
            {
                MsgBox1.alert("DS Refno data " + txtbxDSRefNo.Text + " not found!");
                txtbxDSRefNo.Focus();
            }
            else
            {
                txtbxDSRefNo.Enabled = false;
                txtTransferSlip.Enabled = true;
                txtTransferSlip.Focus();
            }


            bindGrid(dtScanned, gvDeliver);
        }

        public void scanOld()
        {
            Maintenance maint = new Maintenance();
            DataSet ds = new DataSet();

            ds = maint.getDSRefNoDetails(txtbxDSRefNo.Text);

            gvDeliver.DataSource = ds;
            gvDeliver.DataBind();
            if (ds.Tables[0].DefaultView.Count == 0)
            {
                MsgBox1.alert("DS Refno data " + txtbxDSRefNo.Text + " not found!");
            }
            txtbxDSRefNo.Text = "";
            txtbxDSRefNo.Focus();
        }
        protected void btnDeliver_Click(object sender, EventArgs e)
        {

            try
            {
                string strUser = Session["UserID"].ToString();


                if (txtTransferSlip.Text.Length >= 10)
                {
                    DataView dv = aDAL.CHECK_ASP_DS_OR_TS("", txtTransferSlip.Text).Tables[0].DefaultView;
                    string strTS = "";
                    if (dv.Count > 0)
                    {
                        strTS = dv[0]["TS"].ToString();
                    }

                    if (strTS != "")
                    {
                        MsgBox1.alert("Transfer slip was already used!");
                        txtTransferSlip.Text = "";
                        txtTransferSlip.Focus();
                        return;
                    }


                    aDAL.UPDATE_ASP_DELIVER(txtbxDSRefNo.Text.Trim(), txtTransferSlip.Text.Trim(), strUser);

                    Response.Write("<script>");
                    Response.Write("alert('Successfully Saved.');");
                    Response.Write("window.location = 'HTaspDelivery_New2.aspx';");
                    Response.Write("</script>");
                }
                else
                {
                    MsgBox1.alert("Transfer slip must not be less than 10 charaters!");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void saveOLD()
        {
            string iSave = "";
            Maintenance maint = new Maintenance();
            {
                string strUser = Session["UserID"].ToString();
                iSave = maint.UpdateReceivePD(txtbxDSRefNo.Text, strUser);
            }

        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("HTMenu.aspx");
        }

        protected void txtTransferSlip_TextChanged(object sender, EventArgs e)
        {
            DataView dv = aDAL.CHECK_ASP_DS_OR_TS("", txtTransferSlip.Text).Tables[0].DefaultView;
            string strTS = "";
            if (dv.Count > 0)
            {
                strTS = dv[0]["TS"].ToString();
            }

            if (strTS != "")
            {
                MsgBox1.alert("Transfer slip was already used!");
                txtTransferSlip.Text = "";
                txtTransferSlip.Focus();
                return;
            }
        }
    }
}