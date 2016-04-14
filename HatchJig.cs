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
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace QRCodes
{
    class HatchJig : EntityJig
    {
        #region Member variables

        Point3d _previousInsertPoint;
        Point3d _insertPoint;

        double _previousScale;
        double _scale;
        
        bool _pointAcquired = false;

        #endregion

        #region Properties

        public bool PointAcquired
        {
            get { return _pointAcquired; }
            set { _pointAcquired = value; }
        }

        #endregion

        #region Constructor

        public HatchJig(Hatch qrHatch) : base(qrHatch)
        {
            _scale = 4.0;
            _previousScale = 180.0;
        }

        #endregion

        #region Jig inherits methods

        protected override bool Update()
        {
            if (!_pointAcquired) //insert point
            {
                if (_previousInsertPoint.DistanceTo(_insertPoint) < 0.01) return true;
                Matrix3d disp = Matrix3d.Displacement(_previousInsertPoint.GetVectorTo(_insertPoint));
                Entity.TransformBy(disp);
                _previousInsertPoint = _insertPoint;
            }
            else //scale
            {
                if (_scale < 4.0) return true;
                if (_previousScale == _scale) return true;
                Matrix3d scale = Matrix3d.Scaling(1/(_previousScale/_scale), _insertPoint);
                Entity.TransformBy(scale);
                _previousScale = _scale;

            }
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            if (!_pointAcquired) //insert point
            {
                JigPromptPointOptions jigOpts = new JigPromptPointOptions();
                jigOpts.Message = "\nSelect insert point: ";
                jigOpts.UserInputControls = (UserInputControls.Accept3dCoordinates | UserInputControls.NullResponseAccepted | UserInputControls.NoNegativeResponseAccepted);
                jigOpts.BasePoint = Point3d.Origin;
                jigOpts.UseBasePoint = true;
                PromptPointResult jigPoint = prompts.AcquirePoint(jigOpts);
                if (jigPoint.Status != PromptStatus.OK) return SamplerStatus.Cancel;
                if (_insertPoint.DistanceTo(jigPoint.Value) < 0.1)
                {
                    return SamplerStatus.NoChange;
                }
                _insertPoint = jigPoint.Value;
            }
            else //scale
            {
                JigPromptDistanceOptions jigOpts = new JigPromptDistanceOptions();
                jigOpts.UserInputControls = UserInputControls.GovernedByOrthoMode;
                jigOpts.BasePoint = _insertPoint;
                jigOpts.UseBasePoint = true;
                jigOpts.Message = "\nSpecify size: ";
                PromptDoubleResult jigScale = prompts.AcquireDistance(jigOpts);
                if (jigScale.Status != PromptStatus.OK) return SamplerStatus.Cancel;
                double delta = _previousScale - jigScale.Value;
                if (delta < 0.0) delta *= -1;
                if (delta < 0.01) return SamplerStatus.NoChange;
                _scale = jigScale.Value;
            }
            return SamplerStatus.OK;
        }

        #endregion
    }
}
