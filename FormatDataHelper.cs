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
using System.Text;
using System.Text.RegularExpressions;

namespace QRCodes
{
  // Class initially written by Kean Walmsley 
  // Slightly modified here: internal calls to EncodeQrCodeUrl removed
  // Blog post
  // http://through-the-interface.typepad.com/through_the_interface/2010/09/
  // more-fun-with-qr-codes-encoding-different-types-of-data-inside-autocad.html
  // Project source (QrEncoder.cs)
  // http://through-the-interface.typepad.com/files/QrCodes-Project.zip

  static class FormatDataHelper
  {
    // Generate a QR Code URL for calendar event information

    static public string EncodeCalendar(
      string title, DateTime start, DateTime end,
      string location, string desc
    )
    {
      const string eol = "\r\n";

      StringBuilder sb =
        new StringBuilder("BEGIN:VEVENT");

      // The title is mandatory

      sb.AppendFormat(
        "{0}SUMMARY:{1}{2}",
        eol,
        Strip(title),
        eol
      );

      // The other fields are optional

      if (start.Ticks > 0)
      {
        sb.AppendFormat(
          "DTSTART:{0}{1}{2}T{3}{4}{5}Z{6}",
          start.Year.ToString(),
          Pad(start.Month),
          Pad(start.Day),
          Pad(start.Hour),
          Pad(start.Minute),
          Pad(start.Second),
          eol
        );
      }

      if (end.Ticks > 0)
      {
        sb.AppendFormat(
          "DTEND:{0}{1}{2}T{3}{4}{5}Z{6}",
          end.Year.ToString(),
          Pad(end.Month),
          Pad(end.Day),
          Pad(end.Hour),
          Pad(end.Minute),
          Pad(end.Second),
          eol
        );
      }

      if (!String.IsNullOrEmpty(location))
      {
        sb.AppendFormat(
          "LOCATION:{0}{1}",
          Strip(location),
          eol
        );
      }

      if (!String.IsNullOrEmpty(desc))
      {
        sb.AppendFormat(
          "DESCRIPTION:{0}{1}",
          Strip(desc),
          eol
        );
      }

      sb.Append("END:VEVENT");
      sb.Append(eol);

      return sb.ToString();
    }

    // Strip out any full stops
    // (it's unclear why these don't work in
    //  calendar events, but do work fine in other
    //  data types... and the problem is with AutoCAD
    //  rather than Google Charts or ZXing.)

    private static string Strip(string str)
    {
      return str.Replace(".", "");
    }

    // Pad an integer with a leading "0"
    // if needed to make a 2-character string

    private static string Pad(int n)
    {
      return n < 10 ? "0" + n.ToString() : n.ToString();
    }

    // Generate a QR Code URL for contact information

    static public string EncodeContact(
      string name, string phone, string email,
      string address, string address2,
      string website, string memo
    )
    {
      // The name is mandatory

      StringBuilder sb =
        new StringBuilder("MECARD:N:");

      sb.AppendFormat("{0};", name);

      // The other fields are optional

      if (!String.IsNullOrEmpty(phone))
      {
        // Copy the +, if there is one

        sb.Append("TEL:");
        if (phone.StartsWith("+"))
          sb.Append("+");

        // But strip all other non-numeric chars

        sb.Append(Regex.Replace(phone, "\\D", ""));
        sb.Append(";");
      }

      if (!String.IsNullOrEmpty(email))
      {
        sb.AppendFormat("EMAIL:{0};", email);
      }

      if (!String.IsNullOrEmpty(address) ||
          !String.IsNullOrEmpty(address2)
        )
      {
        sb.AppendFormat("ADR:{0}", address);

        // Add a space if both fields exist

        if (
          !String.IsNullOrEmpty(address) &&
          !String.IsNullOrEmpty(address2)
        )
        {
          sb.Append(" ");
        }

        sb.AppendFormat("{0};", address2);
      }

      if (!String.IsNullOrEmpty(website))
      {
        sb.AppendFormat("URL:{0};", website);
      }

      if (!String.IsNullOrEmpty(memo))
      {
        sb.AppendFormat("NOTE:{0};", memo);
      }

      return sb.ToString();
    }

    // Generate a QR Code URL for an email address

    static public string EncodeEmail(string address)
    {
      return "mailto:" + address;
    }

    // Generate a QR Code URL for a geo-location

    static public string EncodeGeolocation(
      double lat, double lng, string query
    )
    {
      string str =
        "geo:" + lat.ToString() + "," + lng.ToString();

      if (!String.IsNullOrEmpty(query))
        str += "?q=" + query;

      return str;
    }

    // Generate a QR Code URL for a phone number

    static public string EncodePhone(string phone)
    {
      return
        "tel:" +
        (phone.StartsWith("+") ? "+" : "") +
        Regex.Replace(phone, "\\D", "")
      ;
    }

    // Generate a QR Code URL for a piece of text

    static public string EncodeText(string text)
    {
      return text;
    }

    // Generate a QR Code URL for a URL

    static public string EncodeUrl(string url)
    {
      return url;
    }

    // Helper function to generate a QR Code URL
    
    static public string EncodeQrCodeUrl(
      string data, string providerHost, string providerPath
    )
    {
      string esc = Uri.EscapeDataString(data);
      esc = esc.Replace(".", "%2E");
      esc = esc.Replace("-", "%2D");
      UriBuilder ub = new UriBuilder();
      ub.Host = providerHost;
      ub.Path = providerPath;
      ub.Query = string.Format("cht=qr&chs=500x500&chl={0}", esc);
      return ub.ToString();
    }

    private const string PROVIDER_HOST = "chart.apis.google.com";
    private const string PROVIDER_PATH = "chart";

    /// <summary>
    /// Helper function to generate a QR Code URL via Google Chart
    /// API for some data. Host and Path are assumed default,
    /// 'chart.apis.google.com' and 'chart' respectively.
    /// </summary>
    /// <param name="data">Data to encode</param>
    /// <returns></returns>
    static public string EncodeQrCodeUrl(string data)
    {
      return EncodeQrCodeUrl(data, PROVIDER_HOST, PROVIDER_PATH);
    }

    static public string EncodeQrCodeDecoderUrl(string url)
    {
      return "http://zxing.org/w/decode?u=" +
        Uri.EscapeDataString(url) +
        "&full=true";
    }
  }
}
