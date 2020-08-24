SELECT e.*
FROM openmrs.episode_patient_program epp 
JOIN openmrs.patient_program pp ON epp.patient_program_id = pp.patient_program_id 
JOIN openmrs.episode_encounter ee ON ee.episode_id = epp.episode_id
JOIN openmrs.encounter e ON ee.encounter_id = e.encounter_id 
WHERE pp.uuid = '${program_enrollment_uuid}'