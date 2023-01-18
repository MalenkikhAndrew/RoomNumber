using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Architecture;

namespace RoomNumber
{
    public class RoomsData
    {
        UIApplication m_revit;
        List<Level> m_levels = new List<Level>();
        List<Room> m_rooms = new List<Room>();
        List<RoomTagType> m_roomTagTypes = new List<RoomTagType>();
        Dictionary<int, List<Autodesk.Revit.DB.Architecture.RoomTag>> m_roomWithTags = new Dictionary<int, List<Autodesk.Revit.DB.Architecture.RoomTag>>();


        public RoomsData(ExternalCommandData commandData)
        {
            m_revit = commandData.Application;
            GetRooms();
            GetRoomTagTypes();
       

        }


        public ReadOnlyCollection<Room> Rooms
        {
            get
            {
                return new ReadOnlyCollection<Room>(m_rooms);
            }
        }


        public ReadOnlyCollection<Level> Levels
        {
            get
            {
                return new ReadOnlyCollection<Level>(m_levels);
            }
        }


        public ReadOnlyCollection<RoomTagType> RoomTagTypes
        {
            get
            {
                return new ReadOnlyCollection<RoomTagType>(m_roomTagTypes);
            }
        }


        private void GetRooms()
        {
            Document document = m_revit.ActiveUIDocument.Document;
            foreach (PlanTopology planTopology in document.PlanTopologies)
            {
                if (planTopology.GetRoomIds().Count != 0 && planTopology.Level != null)
                {
                    m_levels.Add(planTopology.Level);
                    foreach (ElementId eid in planTopology.GetRoomIds())
                    {
                        Room tmpRoom = document.GetElement(eid) as Room;

                        if (document.GetElement(tmpRoom.LevelId) != null && m_roomWithTags.ContainsKey(tmpRoom.Id.IntegerValue) == false)
                        {
                            m_rooms.Add(tmpRoom);
                            m_roomWithTags.Add(tmpRoom.Id.IntegerValue, new List<Autodesk.Revit.DB.Architecture.RoomTag>());
                        }
                    }
                }
            }
        }


        private void GetRoomTagTypes()
        {
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_revit.ActiveUIDocument.Document);
            filteredElementCollector.OfClass(typeof(FamilySymbol));
            filteredElementCollector.OfCategory(BuiltInCategory.OST_RoomTags);
            m_roomTagTypes = filteredElementCollector.Cast<RoomTagType>().ToList<RoomTagType>();
        }



        public void AutoTagRooms(Level level, RoomTagType tagType)
        {
            PlanTopology planTopology = m_revit.ActiveUIDocument.Document.get_PlanTopology(level);

            SubTransaction subTransaction = new SubTransaction(m_revit.ActiveUIDocument.Document);
            subTransaction.Start();
            foreach (ElementId eid in planTopology.GetRoomIds())
            {
                Room tmpRoom = m_revit.ActiveUIDocument.Document.GetElement(eid) as Room;

                if (m_revit.ActiveUIDocument.Document.GetElement(tmpRoom.LevelId) != null && tmpRoom.Location != null)
                {

                    LocationPoint locationPoint = tmpRoom.Location as LocationPoint;
                    UV point = new UV(locationPoint.Point.X, locationPoint.Point.Y);
                    Autodesk.Revit.DB.Architecture.RoomTag newTag = m_revit.ActiveUIDocument.Document.Create.NewRoomTag(new LinkElementId(tmpRoom.Id), point, null);
                    newTag.RoomTagType = tagType;

                    List<Autodesk.Revit.DB.Architecture.RoomTag> tagListInTheRoom = m_roomWithTags[newTag.Room.Id.IntegerValue];
                    tagListInTheRoom.Add(newTag);
                }

            }
            subTransaction.Commit();
        }



        public int GetTagNumber(Room room, RoomTagType tagType)
        {
            int count = 0;
            List<Autodesk.Revit.DB.Architecture.RoomTag> tagListInTheRoom = m_roomWithTags[room.Id.IntegerValue];
            foreach (Autodesk.Revit.DB.Architecture.RoomTag roomTag in tagListInTheRoom)
            {
                if (roomTag.RoomTagType.Id.IntegerValue == tagType.Id.IntegerValue)
                {
                    count++;
                }
            }
            return count;
        }
    }
}

