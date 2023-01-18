using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RoomNumber
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class Command : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData,
                                               ref string message,
                                               ElementSet elements)
        {
            try
            {

                Transaction documentTransaction = new Transaction(commandData.Application.ActiveUIDocument.Document, "Document");
                documentTransaction.Start();

                RoomsData data = new RoomsData(commandData);

                System.Windows.Forms.DialogResult result;


                using (RoomTag roomsTagForm = new RoomTag(data))
                {
                    result = roomsTagForm.ShowDialog();
                }

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    documentTransaction.Commit();
                    return Result.Succeeded;
                }
                else
                {
                    documentTransaction.RollBack();
                    return Result.Cancelled;
                }
            }
            catch (Exception ex)
            {

                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
