param($inputDir)

dir $inputDir -File -Recurse -Force | Group Extension | Sort Name
