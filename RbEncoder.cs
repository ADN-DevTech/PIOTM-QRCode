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
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace QRCodes
{
  // Class initially written by Kean Walmsley 
  // Blog post
  // http://through-the-interface.typepad.com/through_the_interface/2010/09/
  // more-fun-with-qr-codes-encoding-different-types-of-data-inside-autocad.html
  // Project source (RbEncoder.cs)
  // http://through-the-interface.typepad.com/files/QrCodes-Project.zip

  static public class RbEncoder
  {
    // Our registered application name

    const string regApp = "ADNP_QR";

    static public ResultBuffer CreateCalendarRb(
      string title, DateTime start, DateTime end,
      string location, string desc
    )
    {
      ResultBuffer rb =
        new ResultBuffer(
          new TypedValue(1001, regApp),
          new TypedValue(1000, "CAlendar"),
          new TypedValue(1000, "Values"),
          new TypedValue(1000, "Title"),
          new TypedValue(1000, title)
        );
      if (start.Ticks > 0)
      {
        rb.Add(new TypedValue(1000, "Start"));
        rb.Add(new TypedValue(1000, start.ToString()));
      }
      if (end.Ticks > 0)
      {
        rb.Add(new TypedValue(1000, "End"));
        rb.Add(new TypedValue(1000, end.ToString()));
      }
      if (!String.IsNullOrEmpty(location))
      {
        rb.Add(new TypedValue(1000, "Location"));
        rb.Add(new TypedValue(1000, location));
      }
      if (!String.IsNullOrEmpty(desc))
      {
        rb.Add(new TypedValue(1000, "Description"));
        rb.Add(new TypedValue(1000, desc));
      }
      return rb;
    }

    static public ResultBuffer CreateContactRb(
      string name, string phone, string email,
      string address, string address2, string website, string memo
    )
    {
      ResultBuffer rb =
        new ResultBuffer(
          new TypedValue(1001, regApp),
          new TypedValue(1000, "COntact"),
          new TypedValue(1000, "Values"),
          new TypedValue(1000, "Name"),
          new TypedValue(1000, name)
        );
      if (!String.IsNullOrEmpty(phone))
      {
        rb.Add(new TypedValue(1000, "Phone"));
        rb.Add(new TypedValue(1000, phone));
      }
      if (!String.IsNullOrEmpty(email))
      {
        rb.Add(new TypedValue(1000, "Email"));
        rb.Add(new TypedValue(1000, email));
      }
      if (!String.IsNullOrEmpty(address))
      {
        rb.Add(new TypedValue(1000, "Address"));
        rb.Add(new TypedValue(1000, address));
      }
      if (!String.IsNullOrEmpty(address2))
      {
        rb.Add(new TypedValue(1000, "Address2"));
        rb.Add(new TypedValue(1000, address2));
      }
      if (!String.IsNullOrEmpty(website))
      {
        rb.Add(new TypedValue(1000, "Website"));
        rb.Add(new TypedValue(1000, website));
      }
      if (!String.IsNullOrEmpty(memo))
      {
        rb.Add(new TypedValue(1000, "Memo"));
        rb.Add(new TypedValue(1000, memo));
      }
      return rb;
    }

    static public ResultBuffer CreateEmailRb(string email)
    {
      return
        new ResultBuffer(
          new TypedValue(1001, regApp),
          new TypedValue(1000, "Email"),
          new TypedValue(1000, "EValue"),
          new TypedValue(1000, email)
        );
    }

    static public ResultBuffer CreateGeolocationRb(
      double lat, double lng, string query
    )
    {
      ResultBuffer rb =
        new ResultBuffer(
          new TypedValue(1001, regApp),
          new TypedValue(1000, "Geolocation"),
          new TypedValue(1000, "Values"),
          new TypedValue(1000, "Lat"),
          new TypedValue(1040, lat),
          new TypedValue(1000, "Long"),
          new TypedValue(1040, lng)
        );

      if (!String.IsNullOrEmpty(query))
      {
        rb.Add(new TypedValue(1000, "Query"));
        rb.Add(new TypedValue(1000, query));
      }

      return rb;
    }

    static public ResultBuffer CreatePhoneRb(string phone)
    {
      return
        new ResultBuffer(
          new TypedValue(1001, regApp),
          new TypedValue(1000, "Phone"),
          new TypedValue(1000, "PValue"),
          new TypedValue(1000, phone)
        );
    }

    static public ResultBuffer CreateTextRb(string text)
    {
      return
        new ResultBuffer(
          new TypedValue(1001, regApp),
          new TypedValue(1000, "Text"),
          new TypedValue(1000, "TValue"),
          new TypedValue(1000, text)
        );
    }

    static public ResultBuffer CreateUrlRb(string url)
    {
      return
        new ResultBuffer(
          new TypedValue(1001, regApp),
          new TypedValue(1000, "Url"),
          new TypedValue(1000, "UValue"),
          new TypedValue(1000, url)
        );
    }

    public static void AddRegAppTableRecord(string regAppName)
    {
      Document doc =
        Application.DocumentManager.MdiActiveDocument;
      Editor ed = doc.Editor;
      Database db = doc.Database;

      Transaction tr =
        doc.TransactionManager.StartTransaction();
      using (tr)
      {
        RegAppTable rat =
          (RegAppTable)tr.GetObject(
            db.RegAppTableId,
            OpenMode.ForRead,
            false
          );
        if (!rat.Has(regAppName))
        {
          rat.UpgradeOpen();
          RegAppTableRecord ratr =
            new RegAppTableRecord();
          ratr.Name = regAppName;
          rat.Add(ratr);
          tr.AddNewlyCreatedDBObject(ratr, true);
        }
        tr.Commit();
      }
    }
  }
}
