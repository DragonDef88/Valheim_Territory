using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Persistence.Models;
using UnityEngine;

namespace ClanTerritory.Features.Runtime.Restore
{
    internal sealed class RuntimeRestoreMapper
    {
        public RuntimeRestoreSnapshot ToRuntimeSnapshot(
            SaveFileModel saveFile)
        {
            List<RuntimeWardRestoreRecord> wards =
                new List<RuntimeWardRestoreRecord>();

            if (saveFile == null || saveFile.Wards == null)
                return new RuntimeRestoreSnapshot(wards);

            foreach (WardRecord wardRecord in saveFile.Wards)
            {
                RuntimeWardRestoreRecord record =
                    ToRuntimeWardRestoreRecord(wardRecord);

                if (record != null)
                    wards.Add(record);
            }

            return new RuntimeRestoreSnapshot(wards);
        }

        private RuntimeWardRestoreRecord ToRuntimeWardRestoreRecord(
            WardRecord wardRecord)
        {
            if (wardRecord == null)
                return null;

            if (string.IsNullOrEmpty(wardRecord.WardId))
                return null;

            if (wardRecord.Territory == null)
                return null;

            WardId wardId = new WardId(wardRecord.WardId);

            Vector3 position = new Vector3(
                wardRecord.Territory.X,
                wardRecord.Territory.Y,
                wardRecord.Territory.Z);

            return new RuntimeWardRestoreRecord(
                wardId,
                position);
        }
    }
}