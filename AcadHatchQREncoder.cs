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
using System.Collections.Generic;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Util;

namespace QRCodes
{
  class AcadHatchQREncoder : QRCodeEncoder
  {
    #region Constructor

    public AcadHatchQREncoder()
    {
      //default values, may be overwrite
      base.QRCodeEncodeMode = ENCODE_MODE.BYTE;
      base.QRCodeVersion = 7;
      base.QRCodeErrorCorrect = ERROR_CORRECTION.M;
      base.QRCodeScale = 4;
    }

    #endregion

    #region Encode methods

    /// <summary>
    /// Encode the content using the encoding scheme given.
    /// Replace the base.Encode method
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public virtual Hatch HatchEncode(String content)
    {
      if (QRCodeUtility.IsUniCode(content))
      {
        return HatchEncode(content, Encoding.Unicode);
      }
      else
      {
        return HatchEncode(content, Encoding.ASCII);
      }
    }

    /// <summary>
    /// Encode the content using the encoding scheme given.
    /// Replace the base.Encode method
    /// </summary>
    /// <param name="content"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public Hatch HatchEncode(String content, Encoding encoding)
    {
      Hatch h = new Hatch();

      // This for/for/if is the same used at the original
      // ThoughtWorks.Code sample

      bool[][] matrix = calQrcode(encoding.GetBytes(content));
      for (int i = 0; i < matrix.Length; i++)
      {
        for (int j = 0; j < matrix.Length; j++)
        {
          if (matrix[j][i])
          {
            HatchLoop loop = new HatchLoop(HatchLoopTypes.Polyline);

            Point2d[] points =
              new Point2d[]{
                new Point2d(j * QRCodeScale, i * QRCodeScale),
                new Point2d((j+1) * QRCodeScale, i * QRCodeScale),
                new Point2d((j+1) * QRCodeScale, (i+1) * QRCodeScale),
                new Point2d(j * QRCodeScale, (i+1) * QRCodeScale)
              };

            loop.Polyline.Add(new BulgeVertex(points[0], 0));
            loop.Polyline.Add(new BulgeVertex(points[1], 0));
            loop.Polyline.Add(new BulgeVertex(points[2], 0));
            loop.Polyline.Add(new BulgeVertex(points[3], 0));
            loop.Polyline.Add(new BulgeVertex(points[0], 0));

            h.AppendLoop(loop);
          }
        }
      }

      // Mirror the hatch as the original code use Windows
      // coordinates (from upper left corner)

      double halfSize = (h.GeometricExtents.MaxPoint.X - 
          h.GeometricExtents.MinPoint.X)/2;
      h.TransformBy(
        Matrix3d.Mirroring(
          new Line3d(
            new Point3d(halfSize, 0, 0),
            new Point3d(halfSize, halfSize, 0)
          )
        )
      );

      return h;
    }

    #endregion
  }
}
