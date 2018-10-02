# Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline
A common pattern I have seen customers implementing in their DevOps strategy is to have two seperate pipelines for Dev and QA-Prod.  Their Dev pipeline is full CI/CD so each source control check-in triggers a build and also triggers their release pipeline.  This pipeline only has one stage named Dev and then a entirely seperate pipeline is created for QA/Production.  The main reason I have heard from customers for the two pipelines versus just one, is that they are worried a check-in to source control will possibly make it to QA or Production when the customer is not ready for the release.  Also, they do not want to "reject" every single CI/CD release to Dev from making it to QA.  

## Ideal pattern:
1. Code is checked into source control
2. A build is automatically started and the code is compiled 
3. When the build completes (successfully) the release pipeline is started (CI/CD)
4. The new code is pushed to Development
5. Once the code is verified, the new code is approved for a QA release
6. Once the QA release is verified, the new code is approved for a Production release

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/Ideal-Pipeline.png)

## The Issue:
If you have lots source control check-ins through out the day, you get the above steps executing each and every time, but you really do not want to reject the code to prevent a release to QA for each check in.  Your apporval person will probably get pretty annoyed.  

## What I have seen:
Customer will build two pipelines.
1. Dev
  a. Steps 1 through 4 from above still apply, but there is no link to QA.  
     ![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/SeperateDevRelease.png)

2. QA-Prod
  a. Steps 5 and 6 are put in a different pipeline which needs to be kicked off.  And even worse I have seen step 2 (a new build) occur in this pipeline.  You should only build your code once!  Building for Dev and then building for QA-Prod introduces the possiblility of different code being compiled.
     ![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/SeperateQAProdRelease.png)

## The solution
1. Implement the Ideal pipeline
2. In Step 5 add the following:
  a. Keep our normal approval process to QA
  b. Add a Gate
  c. The Gate will wait unti the Build has a Bulid Tag applied
  d. Once the Build tag is applied the release will continue
  e. The approval to QA is performed and everything works as normal

### How to setup the build tag gate
1. Copy the Azure Function that is in this repository
2. Click on the Pre-Deployment Conditions for QA
3. Add your approvers
4. Add a Function Gate
5. In the Body enter:
   ```
   {
    "personalAcccessToken": "$(personalAcccessToken)",
    "organization" : "$(organization)",
    "project" : "$(build.projectname)",
    "buildId" : "$(build.buildid)",
    "requiredTag" : "$(requiredTag)"
   }
   ```
 6. Create variables for personalAcccessToken, organization and requiredTag
    1. personalAcccessToken = you will need to generate a PAT token
    2. organization = this is part of your url 
    3. requiredTag = this is the tag to test on your build (e.g. ApprovedForRelease)
 7. For the Success Criteria enter: eq(root['status'], 'successful')
 8. Under Evaluation Options select "On Successful Gates, ask for approvals" (we want the gates to execute first, before approvals).
 9. You should set how long you want this gate to re-try.  This depends on how long you typically wait between a dev release and you push to QA.
 10. TO DO: Evaluate how many parrallel jobs are needed!!!!!!!!!!!!!!

#### QA Gate approvals
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Gate-Approval-Setting.png)

#### QA Function Gate
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Gate-1.png)

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Gate-2.png)

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Gate-Approval-Setting.png)

#### QA Gate failure since Build is not Tagged
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/Gate-Failed.png)

#### Tagging the Build
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/AddABuildTag.png)

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/BuildTagAdded.png)

#### QA Gate passed, waiting for approvals
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Waiting-For-Approval.png)
