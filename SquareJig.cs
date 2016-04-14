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
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace QRCodes
{
  class SquareJig : EntityJig
  {
    Point3d _secPt;
    Point3d _fstPt;

    /// <summary>
    /// Lower left point of the square
    /// </summary>
    public Point3d LowerLeftCorner
    {
      get
      {
        Polyline pline = base.Entity as Polyline;
        Point3d firstPoint = pline.GetPoint3dAt(0);
        Point3d secondPoint = pline.GetPoint3dAt(2);
        if (secondPoint.Y < firstPoint.Y) return secondPoint;
        return firstPoint;
      }
    }

    /// <summary>
    /// Internal: Real width (signed)
    /// </summary>
    private double Width
    {
      get
      {
        return _secPt.GetVectorTo(_fstPt).X;
      }
    }

    /// <summary>
    /// Internal: Real height (signed)
    /// </summary>
    private double Height
    {
      get
      {
        return _secPt.GetVectorTo(_fstPt).Y;
      }
    }

    /// <summary>
    /// The final size of the square
    /// </summary>
    public double Size
    {
      get
      {
        return Math.Min(Math.Abs(Width), Math.Abs(Height));
      }
    }

    public SquareJig(Point3d first)
      : base(new Polyline())
    {
      // Store the first point
      
      _fstPt = first;

      // And draw a pline using this point
      
      Point2d firstPointPlane = first.Convert2d(new Plane());
      Polyline pline = base.Entity as Polyline;
      pline.AddVertexAt(0, firstPointPlane, 0, 0, 0);
      pline.AddVertexAt(
        1, firstPointPlane.Add(new Vector2d(10, 0)), 0, 0, 0
      );
      pline.AddVertexAt(
        2, firstPointPlane.Add(new Vector2d(10, 10)), 0, 0, 0
      );
      pline.AddVertexAt(
        3, firstPointPlane.Add(new Vector2d(0, 10)), 0, 0, 0
      );
      pline.Closed = true;
    }

    protected override bool Update()
    {
      // Get the dimension signs (positive or negative)
      
      double widthSign = (Width < 0 ? 1 : -1);
      double heightSign = (Height < 0 ? 1 : -1);

      // Resize the pline using the Size and the signs
      
      Polyline pline = base.Entity as Polyline;
      pline.SetPointAt(
        1,
        new Point2d(_fstPt.X + Size * widthSign, _fstPt.Y)
      );
      pline.SetPointAt(
        2,
        new Point2d(
          _fstPt.X + Size * widthSign,
          _fstPt.Y + Size * heightSign
        )
      );
      pline.SetPointAt(
        3,
        new Point2d(_fstPt.X, _fstPt.Y + Size * heightSign)
      );
      return true;
    }

    protected override SamplerStatus Sampler(JigPrompts prompts)
    {
      JigPromptPointOptions jigOpts =
        new JigPromptPointOptions();
      jigOpts.Message = "\nSelect second point: ";
      jigOpts.UserInputControls =
        (UserInputControls.NullResponseAccepted |
         UserInputControls.NoNegativeResponseAccepted);
      jigOpts.BasePoint = Point3d.Origin;
      jigOpts.UseBasePoint = true;
      PromptPointResult jigPoint = prompts.AcquirePoint(jigOpts);
      if (jigPoint.Status != PromptStatus.OK)
        return SamplerStatus.Cancel;
      if (_secPt.DistanceTo(jigPoint.Value) < 0.0001)
      {
        return SamplerStatus.NoChange;
      }
      _secPt = jigPoint.Value;
      return SamplerStatus.OK;
    }
  }
}
