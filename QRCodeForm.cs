// (C) Copyright 2010 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

using System;
using System.Globalization;
using System.Windows.Forms;
using ThoughtWorks.QRCode.Codec;
using Autodesk.AutoCAD.DatabaseServices;

namespace QRCodes
{
  public partial class QRCodeForm : Form
  {
    QRCodeHatchAdvancedOptionsForm _advOpts;

    public QRCodeForm()
    {
      InitializeComponent();
      _advOpts = new QRCodeHatchAdvancedOptionsForm();
    }

    private bool _isEditing;

    public bool IsEditing
    {
      get { return _isEditing; }
      set { _isEditing = value; }
    }

    private static string Pad(int n)
    {
      return n < 10 ? "0" + n.ToString() : n.ToString();
    }

    /// <summary>
    /// Text to encode typed by the user
    /// </summary>
    public string QREncodeData
    {
      get
      {
        switch (cboDataType.SelectedIndex)
        {
          case 0: // Calendar
            return FormatDataHelper.EncodeCalendar(
              txtEventTitle.Text, txtEventStart.Value,
              txtEventEnd.Value, txtEventLocation.Text,
              txtEventDescription.Text
            );
          case 1: // Contact
            return FormatDataHelper.EncodeContact(
              txtContactName.Text, txtContactPhone.Text,
              txtContactEmail.Text, txtContactAddress1.Text,
              txtContactAddress2.Text, txtContactWebsite.Text,
              txtContactMemo.Text
            );
          case 2: // Email
            return txtEmail.Text;
          case 3: // geolocation
            double locationLat = double.Parse(txtLocationLat.Text);
            double locationLong = double.Parse(txtLocationLong.Text);
            return FormatDataHelper.EncodeGeolocation(
              locationLat, locationLong, txtLocationQuery.Text
            );
          case 4: // Phone
            return txtPhoneNumber.Text;
          case 5: // Text
            return txtText.Text;
          case 6: // Url
            return txtURL.Text;
        }
        return string.Empty;
      }
    }

    public ResultBuffer QREncodeDataAsResultBuffer
    {
      get
      {
        switch (cboDataType.SelectedIndex)
        {
          case 0: // Calendar
            return RbEncoder.CreateCalendarRb(
              txtEventTitle.Text, txtEventStart.Value,
              txtEventEnd.Value, txtEventLocation.Text,
              txtEventDescription.Text
            );
          case 1: // Contact
            return RbEncoder.CreateContactRb(
              txtContactName.Text, txtContactPhone.Text,
              txtContactEmail.Text, txtContactAddress1.Text,
              txtContactAddress2.Text, txtContactWebsite.Text, 
              txtContactMemo.Text
            );
          case 2: // Email
            return RbEncoder.CreateEmailRb(txtEmail.Text);
          case 3: // Geolocation
            double locationLat = double.Parse(txtLocationLat.Text);
            double locationLong = double.Parse(txtLocationLong.Text);
            return RbEncoder.CreateGeolocationRb(
              locationLat, locationLong, txtLocationQuery.Text
            );
          case 4: // Phone
            return RbEncoder.CreatePhoneRb(txtPhoneNumber.Text);
          case 5: // Text
            return RbEncoder.CreateTextRb(txtText.Text);
          case 6: // Url
            return RbEncoder.CreateUrlRb(txtURL.Text);
        }
        return null;
      }
      set
      {
        TypedValue[] vals = value.AsArray();
        switch ((string)vals[1].Value)
        {
          case "CAlendar":
            cboDataType.SelectedIndex = 0;
            txtEventTitle.Text =
              TryGetTPValsArrayValue(vals, 4);
            txtEventStart.Value =
              DateTime.Parse(TryGetTPValsArrayValue(vals, 6));
            txtEventEnd.Value =
              DateTime.Parse(TryGetTPValsArrayValue(vals, 8));
            txtEventLocation.Text =
              TryGetTPValsArrayValue(vals, 10);
            txtEventDescription.Text =
              TryGetTPValsArrayValue(vals, 12);
            break;
          case "COntact":
            cboDataType.SelectedIndex = 1;
            txtContactName.Text =
              TryGetTPValsArrayValue(vals, 4);
            txtContactPhone.Text =
              TryGetTPValsArrayValue(vals, 6);
            txtContactEmail.Text =
              TryGetTPValsArrayValue(vals, 8);
            txtContactAddress1.Text =
              TryGetTPValsArrayValue(vals, 10);
            txtContactAddress2.Text =
              TryGetTPValsArrayValue(vals, 12);
            txtContactWebsite.Text =
              TryGetTPValsArrayValue(vals, 14);
            txtContactMemo.Text =
              TryGetTPValsArrayValue(vals, 16);
            break;
          case "Email":
            cboDataType.SelectedIndex = 2;
            txtEmail.Text = TryGetTPValsArrayValue(vals, 3);
            break;
          case "Geolocation":
            cboDataType.SelectedIndex = 3;
            txtLocationLat.Text =
              TryGetTPValsArrayValue(vals, 4);
            txtLocationLong.Text =
              TryGetTPValsArrayValue(vals, 6);
            txtLocationQuery.Text =
              TryGetTPValsArrayValue(vals, 8);
            break;
          case "Phone":
            cboDataType.SelectedIndex = 4;
            txtPhoneNumber.Text =
              TryGetTPValsArrayValue(vals, 3);
            break;
          case "Text":
            cboDataType.SelectedIndex = 5;
            txtText.Text =
              TryGetTPValsArrayValue(vals, 3);
            break;
          case "Url":
            cboDataType.SelectedIndex = 6;
            txtURL.Text =
              TryGetTPValsArrayValue(vals, 3);
            break;
        }
      }
    }

    private string TryGetTPValsArrayValue(
      TypedValue[] tpVals, int index
    )
    {
      if (tpVals.Length < index)
        return string.Empty;
      return (string)tpVals[index].Value;
    }

    /// <summary>
    /// Encode mode selected by the user
    /// </summary>
    public QRCodeEncoder.ENCODE_MODE QREncode
    {
      get
      {
        String encoding = _advOpts.cboEncoding.Text;
        if (encoding == "Byte")
        {
          return QRCodeEncoder.ENCODE_MODE.BYTE;
        }
        else if (encoding == "AlphaNumeric")
        {
          return QRCodeEncoder.ENCODE_MODE.ALPHA_NUMERIC;
        }
        else if (encoding == "Numeric")
        {
          return QRCodeEncoder.ENCODE_MODE.NUMERIC;
        }
        return QRCodeEncoder.ENCODE_MODE.BYTE; // Default
      }
    }

    /// <summary>
    /// Scale selected by the user
    /// </summary>
    public int QRSize
    {
      get
      {
        return Convert.ToInt16(txtSize.Text);
      }
    }

    public bool QRSacelOnScreen
    {
      get
      {
        return chkScaleOnScreen.Checked;
      }
    }

    /// <summary>
    /// Version selected by the user
    /// </summary>
    public int QRVersion
    {
      get
      {
        return Convert.ToInt16(_advOpts.cboVersion.Text);
      }
    }

    /// <summary>
    /// Error correct mode selected by the user
    /// </summary>
    public QRCodeEncoder.ERROR_CORRECTION QRErrorCorrect
    {
      get
      {
        string errorCorrect = _advOpts.cboCorrectionLevel.Text;
        if (errorCorrect == "L")
          return QRCodeEncoder.ERROR_CORRECTION.L;
        else if (errorCorrect == "M")
          return QRCodeEncoder.ERROR_CORRECTION.M;
        else if (errorCorrect == "Q")
          return QRCodeEncoder.ERROR_CORRECTION.Q;
        else if (errorCorrect == "H")
          return QRCodeEncoder.ERROR_CORRECTION.H;
        return QRCodeEncoder.ERROR_CORRECTION.M; //default
      }
    }

    public enum QRType
    {
      /// <summary>
      /// Native AutoCAD Hatch
      /// </summary>
      Hatch,
      /// <summary>
      /// Online image
      /// </summary>
      Online
    }

    /// <summary>
    /// The entity type selected by the user
    /// </summary>
    public QRType QREntityType
    {
      get
      {
        if (radNativeHatch.Checked) return QRType.Hatch;
        return QRType.Online;
      }
    }

    private void QRCodeForm_Load(object sender, EventArgs e)
    {
      _advOpts.cboEncoding.SelectedIndex = 2;
      _advOpts.cboVersion.SelectedIndex = 6;
      _advOpts.cboCorrectionLevel.SelectedIndex = 1;

      radNativeHatch.Checked = true;
      btnOptionsOnline.Enabled = false;
      chkScaleOnScreen.Checked = true;

      // Current culture date format

      DateTimeFormatInfo dtfi =
        Application.CurrentCulture.DateTimeFormat;
      string currentDateFormat =
        dtfi.ShortDatePattern + " " + dtfi.LongTimePattern;
      txtEventStart.CustomFormat = currentDateFormat;
      txtEventEnd.CustomFormat = currentDateFormat;

      if (IsEditing)
      {
        btnGenerate.Text = "Save";
        
        // Disable options, only context editing allowed

        grpOptions.Enabled = false;
      }
      else
      {
        txtEventStart.Value = DateTime.Now;
        txtEventEnd.Value = DateTime.Now.AddHours(1);

        cboDataType.SelectedIndex = 5;
      }
    }

    private void btnGenerate_Click(object sender, EventArgs e)
    {
      // We can use string.IsNullOrWhiteSpace,
      // but .NET 4.0 does not work until AutoCAD 2011
      
      switch (cboDataType.SelectedIndex)
      {
        case 0: // Calendar
          break;
        case 1: // Contact
          break;
        case 2: // Email
          break;
        case 3: // Geolocation
          break;
        case 4: // Phone
          if (txtPhoneNumber.Text.Trim().Length == 0)
          {
            MessageBox.Show(
              "Phone number field must not be empty", "QR Generator",
              MessageBoxButtons.OK, MessageBoxIcon.Error
            );
            return;
          }
          break;
        case 5: // Text
          if (txtText.Text.Trim().Length == 0)
          {
            MessageBox.Show(
              "Text field must not be empty", "QR Generator",
              MessageBoxButtons.OK, MessageBoxIcon.Error
            );
            return;
          }
          break;
        case 6: // Url
          if (txtURL.Text.Trim().Length == 0)
          {
            MessageBox.Show(
              "URL field must not be empty", "QR Generator",
              MessageBoxButtons.OK, MessageBoxIcon.Error
            );
            return;
          }
          break;
      }

      DialogResult = DialogResult.OK;
      Close();
    }

    #region Controls enable/disable

    private void chkScaleOneScreen_CheckedChanged(
      object sender, EventArgs e
    )
    {
      txtSize.Enabled = !chkScaleOnScreen.Checked;
    }

    private void btnOptionsHatch_Click(object sender, EventArgs e)
    {
      Autodesk.AutoCAD.ApplicationServices.Application.
        ShowModalDialog(_advOpts);
    }

    private void radNativeHatch_CheckedChanged(
      object sender, EventArgs e
    )
    {
      btnOptionsHatch.Enabled = radNativeHatch.Checked;
    }

    private void radOnlineImage_CheckedChanged(
      object sender, EventArgs e
    )
    {
      btnOptionsOnline.Enabled = radOnlineImage.Checked;
    }

    private void cboDataType_SelectedIndexChanged(
      object sender, EventArgs e
    )
    {
      tabDataType.SelectedIndex = cboDataType.SelectedIndex;
    }

    #endregion

  }
}
