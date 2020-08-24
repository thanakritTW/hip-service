SELECT o.*
FROM openmrs.episode_patient_program epp 
join openmrs.patient_program pp on epp.patient_program_id = pp.patient_program_id 
join openmrs.episode_encounter ee on ee.episode_id = epp.episode_id 
join openmrs.obs o on o.encounter_id = ee.encounter_id 
where pp.uuid = '${program_enrollment_uuid}'
