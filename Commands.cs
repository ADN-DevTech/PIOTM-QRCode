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
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ThoughtWorks.QRCode.Codec;

namespace QRCodes
{
  public class Commands
  {
    const string APPLICATION_PREFIX = "ADNP_QR";

    #region Command methods

    [CommandMethod("ADNPLUGINS", "-QRGEN", CommandFlags.Modal)]
    public static void GenerateQRByCommandLine()
    {
      Editor ed =
        Application.DocumentManager.MdiActiveDocument.Editor;

      //Obtain type of QR Code

      PromptKeywordOptions pko =
          new PromptKeywordOptions(
              "\nType of entity ");
      pko.Keywords.Add("Native");
      pko.Keywords.Add("Online");

      PromptResult pkr =
          ed.GetKeywords(pko);
      if (pkr.Status != PromptStatus.OK)
        return;

      //Obtain data from user

      ResultBuffer def = null; //this is used for editing
                               //ignore for this command
      ResultBuffer rb; //stores the information for late edit
      string qrData = CommandLineHelper.GetTextForQrCode(
          ed, def, out rb);

      //Obtain the insert point and size

      Point3d insertPoint = Point3d.Origin;
      double size = 180;
      if (
        PromptInsertPointAndSize(
          true, ref insertPoint, ref size) !=
          PromptStatus.OK
        ) return;

      //generate the qr code entity

      Entity ent = null;
      switch (pkr.StringResult)
      {
        case "Native":
          ent = GenerateQRHatch(qrData,
            QRCodeEncoder.ENCODE_MODE.ALPHA_NUMERIC,
            7, QRCodeEncoder.ERROR_CORRECTION.M, size);
          if (ent == null) return;
          break;
        case "Online":
          string uri =
            FormatDataHelper.EncodeQrCodeUrl(qrData);
          ent = CreateRasterImage(uri, size);
          break;
      }

      // Append to the current space

      AppendEntityToCurrentSpace(ent);

      // Add the xdata and move the ent to the correct location

      SetXDataAndMoveToLocation(
        ent, rb, insertPoint);
    }

    [CommandMethod("ADNPLUGINS", "QRGEN", CommandFlags.Modal)]
    public static void GenerateQRByForm()
    {
      // Show the form
      QRCodeForm form = new QRCodeForm();
      System.Windows.Forms.DialogResult res =
        Application.ShowModalDialog(form);
      if (res != System.Windows.Forms.DialogResult.OK) return;

      // Obtain insert point and size

      Point3d insertPoint = Point3d.Origin;
      double size = form.QRSize;
      if (
        PromptInsertPointAndSize(
          form.QRSacelOnScreen,
          ref insertPoint, ref size) != PromptStatus.OK
        )
        return;

      // Generate the QR entity

      Entity ent = null;
      switch (form.QREntityType)
      {
        case QRCodeForm.QRType.Hatch:

          // Generate the QR as a Hatch entity

          ent = GenerateQRHatch(
            form.QREncodeData, form.QREncode,
            form.QRVersion, form.QRErrorCorrect, size
          );
          if (ent == null) return;

          // Append to the current space

          AppendEntityToCurrentSpace(ent);
          break;

        case QRCodeForm.QRType.Online:

          // Generate the QR as an ONline Raster Image entity

          string uri =
            FormatDataHelper.EncodeQrCodeUrl(form.QREncodeData);
          ent = CreateRasterImage(uri, size);
          break;
      }

      if (ent == null) return;

      // Add the xdata and move the ent to the correct location

      SetXDataAndMoveToLocation(
        ent, form.QREncodeDataAsResultBuffer, insertPoint
      );
    }

    private static void SetXDataAndMoveToLocation(
      Entity ent, ResultBuffer rbdata, Point3d pt
    )
    {
      Database db =
        Application.DocumentManager.MdiActiveDocument.Database;
      Transaction tr = db.TransactionManager.StartTransaction();
      using (tr)
      {
        ent =
          tr.GetObject(ent.ObjectId, OpenMode.ForWrite) as Entity;

        // Let's add our message information as XData,
        // for later editing

        RbEncoder.AddRegAppTableRecord(APPLICATION_PREFIX);
        ResultBuffer rb = rbdata;
        ent.XData = rb;
        rb.Dispose();

        // Move to the correct location

        Matrix3d disp = Matrix3d.Displacement(pt.GetAsVector());
        ent.TransformBy(disp);

        tr.Commit();
      }
    }

    private static RasterImage CreateRasterImage(string uri, double sz)
    {
      Document doc =
        Application.DocumentManager.MdiActiveDocument;
      Editor ed = doc.Editor;
      Database db = doc.Database;

      Transaction tr = db.TransactionManager.StartTransaction();
      using (tr)
      {
        // Get the image dictionary's ID, if it already exists

        ObjectId dictId = RasterImageDef.GetImageDictionary(db);

        if (dictId.IsNull) // If it doesn't, create a new one
          dictId = RasterImageDef.CreateImageDictionary(db);

        // Open the image dictionary

        DBDictionary dict =
          (DBDictionary)tr.GetObject(dictId, OpenMode.ForRead);

        // Get a unique record name for our raster image definition

        int i = 0;
        string defName =
          APPLICATION_PREFIX + i.ToString();

        while (dict.Contains(defName))
        {
          i++;
          defName = APPLICATION_PREFIX + i.ToString();
        }
        RasterImageDef rid = new RasterImageDef();
        try
        {
          // Set its source image and load it
          rid.SourceFileName = uri;
          rid.Load();
        }
        catch
        {
          ed.WriteMessage(
            "\nUnable to create image object. " +
            "Here is the URL to the image: {0}",
            uri
          );
          System.Diagnostics.Process.Start(uri);
          return null;
        }

        // Put the definition in the dictionary

        dict.UpgradeOpen();
        ObjectId defId = dict.SetAt(defName, rid);

        // Let the transaction know about it

        tr.AddNewlyCreatedDBObject(rid, true);

        RasterImage ri = new RasterImage();
        ri.ImageDefId = defId;

        // Resize

        ri.TransformBy(Matrix3d.Scaling(sz / 500.0, Point3d.Origin));

        AppendEntityToCurrentSpace(ri);

        // Create a reactor between the RasterImage and the
        // RasterImageDef to avoid the "unreferenced" 
        // warning in the XRef palette

        RasterImage.EnableReactors(true);
        ri.AssociateRasterDef(rid);

        tr.Commit();

        return ri;
      }
    }

    [CommandMethod("ADNPLUGINS", "QRED", CommandFlags.Modal)]
    public static void EditQR()
    {
      Document doc =
        Application.DocumentManager.MdiActiveDocument;
      Editor ed = doc.Editor;
      Database db = doc.Database;

      // Ask user to select an QR code, hatch or raster image

      PromptEntityOptions peo =
        new PromptEntityOptions("Select a QR code: ");
      peo.SetRejectMessage(
        "\nMust be a hatch or a raster image"
      );
      peo.AddAllowedClass(typeof(Hatch), true);

      // AutoCAD crash if we try AddAllowedClass for RasterImage
      // when no raster image is defined or just hatch QRs were
      // defined, probably because in C++ we need to call
      // acedArxLoad("acismui.arx"), which is not exposed in .NET,
      // so let's check before if the drawing contains any
      // RasterImages, if not we don't need this filter.

      if (!RasterImageDef.GetImageDictionary(db).IsNull)
        peo.AddAllowedClass(typeof(RasterImage), true);
      PromptEntityResult entityResult = ed.GetEntity(peo);
      if (entityResult.Status != PromptStatus.OK) return;

      Transaction tr = db.TransactionManager.StartTransaction();
      using (tr)
      {
        Entity ent =
          tr.GetObject(entityResult.ObjectId, OpenMode.ForRead)
          as Entity;
        ResultBuffer rb =
          ent.GetXDataForApplication(APPLICATION_PREFIX);

        if (rb != null && rb.AsArray().Length == 0)
        {
          ed.WriteMessage("\nThis is not a valid QR code");
          tr.Commit(); //faster
          return;
        }

        // Show the form with current information

        QRCodeForm form = new QRCodeForm();
        form.IsEditing = true;
        form.QREncodeDataAsResultBuffer = rb;
        rb.Dispose();
        System.Windows.Forms.DialogResult res =
          Application.ShowModalDialog(form);
        if (res != System.Windows.Forms.DialogResult.OK) return;

        //Get insert point and size

        double size =
          ent.GeometricExtents.MaxPoint.X -
          ent.GeometricExtents.MinPoint.X;
        Point3d inspt = ent.GeometricExtents.MinPoint;


        if (ent is RasterImage)
        {
          // Just update the raster image definition

          RasterImage image = ent as RasterImage;
          RasterImageDef imageDef =
            tr.GetObject(image.ImageDefId, OpenMode.ForWrite)
            as RasterImageDef;
          imageDef.SourceFileName =
            FormatDataHelper.EncodeQrCodeUrl(form.QREncodeData);
          imageDef.Load();
        }
        else
        {
          // Erase current entity

          ent.UpgradeOpen();
          ent.Erase();

          // Create a new one

          Entity newEnt =
            GenerateQRHatch(
              form.QREncodeData, form.QREncode,
              form.QRVersion, form.QRErrorCorrect, (int)size
            );
          if (newEnt == null) return;
          ResultBuffer newRb = form.QREncodeDataAsResultBuffer;
          newEnt.XData = newRb;
          newRb.Dispose();
          newEnt.TransformBy(
            Matrix3d.Displacement(inspt.GetAsVector())
          );
          AppendEntityToCurrentSpace(newEnt);
        }

        tr.Commit();
      }
    }

    public static PromptStatus PromptInsertPointAndSize(
      bool enableJig, ref Point3d insertPoint, ref double size
    )
    {
      Editor ed =
        Application.DocumentManager.MdiActiveDocument.Editor;
      if (enableJig) // Use Jig
      {
        // First point

        PromptPointResult ppr =
          ed.GetPoint("Select first point: ");
        if (ppr.Status != PromptStatus.OK)
          return PromptStatus.Cancel;

        // Show important message

        ed.WriteMessage(
          "\nThe QR code must be square. " +
          "Smaller dimension will be used.\n"
        );

        // Do JIG for second point

        SquareJig sqJig = new SquareJig(ppr.Value);
        PromptResult pr = ed.Drag(sqJig);
        if (pr.Status != PromptStatus.OK)
          return PromptStatus.Cancel;

        insertPoint = sqJig.LowerLeftCorner;
        size = sqJig.Size;
      }
      else // No Jig, faster
      {
        // Prompt for insert point

        PromptPointResult ppr = ed.GetPoint("\nSelect insert point: ");
        if (ppr.Status != PromptStatus.OK) return PromptStatus.Cancel;

        insertPoint = ppr.Value;

        //size = //specified on form
      }
      return PromptStatus.OK;
    }

    #endregion

    #region Hatch Jig method

    public static PromptStatus DoJig(Hatch qrh)
    {
      HatchJig jig = new HatchJig(qrh);

      Editor ed =
        Application.DocumentManager.MdiActiveDocument.Editor;

      PromptResult res = ed.Drag(jig);
      if (res.Status == PromptStatus.OK)
      {
        // Insert point selected
        // Let's select the scale

        jig.PointAcquired = true;
        res = ed.Drag(jig);
        return res.Status;
      }
      return res.Status;
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Generate the QR Hatch using default values
    /// </summary>
    /// <param name="textToEncode">Text to encode</param>
    private static Hatch GenerateQRHatch(string textToEncode)
    {
      //using the ThoughtWorks.Code sample default values 
      return GenerateQRHatch(textToEncode,
          QRCodeEncoder.ENCODE_MODE.BYTE,
          7, QRCodeEncoder.ERROR_CORRECTION.M, 180);
    }

    /// <summary>
    /// Generate the QR Hatch
    /// </summary>
    /// <param name="textToEncode">Text to encode</param>
    /// <param name="encode">Encode mode</param>
    /// <param name="version">Version of the encoding</param>
    /// <param name="errorCorrect">Error correct mode</param>
    /// <param name="scale">Scale</param>
    private static Hatch GenerateQRHatch(
      string textToEncode,
      QRCodeEncoder.ENCODE_MODE encode,
      int version,
      QRCodeEncoder.ERROR_CORRECTION errorCorrect,
      double size
    )
    {
      // Create the HATCH with the QR Code

      AcadHatchQREncoder qre = new AcadHatchQREncoder();
      qre.QRCodeEncodeMode = encode;
      qre.QRCodeVersion = version;
      qre.QRCodeErrorCorrect = errorCorrect;
      qre.QRCodeScale = 1;//scale;

      Hatch qrHatch = null;
      bool autoIncreaseVersion = false;
      while (qrHatch == null)
      {
        try
        {
          qrHatch = qre.HatchEncode(textToEncode);
        }
        catch
        {
          Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
          if (autoIncreaseVersion)
          {
            //security check: this code cannot generate qr code
            //with version higher than 40 (limitation)
            //at version 40 qith medium correction level, the
            //size for alpha numeric is 3391 chars
            //http://www.denso-wave.com/qrcode/vertable1-e.html

            if (qre.QRCodeVersion > 40)
            {
              ed.WriteMessage("Impossible generate a " +
                  "QR code hatch at this version. Please " +
                  "review and reduce your data and try again");
              return null;
            }

            // keep increasing....

            qre.QRCodeVersion++;
            continue;
          }
          PromptKeywordOptions pko = new PromptKeywordOptions(
              string.Format("\nImpossible create hatch with version {0}. " +
              "Would you like to increase?\nImportant: higher version " +
              "number generate more dense QR codes, which can affect " +
              "the reading/decoding process.", qre.QRCodeVersion));
          pko.Keywords.Add("No");
          pko.Keywords.Add("Yes");
          PromptResult pkr = ed.GetKeywords(pko);
          if (pkr.Status != PromptStatus.OK)
            return null;
          if (pkr.StringResult.Equals("No"))
            return null;
          qre.QRCodeVersion++;
          autoIncreaseVersion = true;
        }
      }

      // Configure and evaluate the hatch

      qrHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
      qrHatch.EvaluateHatch(true);

      //get the min and max point of the hatch

      Point3d minPt = qrHatch.GeometricExtents.MinPoint;
      Point3d maxPt = qrHatch.GeometricExtents.MaxPoint;

      //the center point used for scaling, and the scale factor

      Point3d centerPoint = minPt;
      double currentSize = maxPt.X - minPt.X;
      double scaleFactor = (double)size / currentSize;

      //now resize the hatch

      qrHatch.TransformBy(Matrix3d.Scaling(scaleFactor, centerPoint));

      return qrHatch;
    }

    /// <summary>
    /// Append the entity to the current space of the current dabatabase
    /// </summary>
    /// <param name="entity">The hatch to append</param>
    private static void AppendEntityToCurrentSpace(Entity entity)
    {
      if (!entity.ObjectId.IsNull) return; //already in db

      Database db =
        Application.DocumentManager.MdiActiveDocument.Database;

      Transaction tr = db.TransactionManager.StartTransaction();
      using (tr)
      {
        // Append to the current space

        BlockTableRecord btr =
          tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite)
          as BlockTableRecord;
        btr.AppendEntity(entity);
        tr.AddNewlyCreatedDBObject(entity, true);

        tr.Commit();
      }
    }

    #endregion
  }
}
