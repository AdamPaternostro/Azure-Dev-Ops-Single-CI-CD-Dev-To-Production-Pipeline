# Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline
A common pattern I have seen customers implementing in their DevOps strategy is to have two separate pipelines for Dev and QA-Prod.  Their Dev pipeline is full CI/CD process, so each source control check-in triggers a build which then triggers their release pipeline.  The Dev pipeline only has one stage named Dev and an entirely separate pipeline exists for QA/Production.  The main reason I have heard for having two pipelines (versus just one), is that customers are worried a check-in to source control will mistakely be deployed to QA/Production.  Also, customers with approvals in their pipelines do not want to "reject" all the Dev releases that have no change of making it past Dev.  Here is how I deal with the issue.

## Ideal build/release pattern:
1. Code is checked into source control
2. A build is automatically started and the code is compiled 
3. When the build completes (successfully) the release pipeline is started 
4. The new code is pushed to Development
5. Once the code is verified, the new code is approved for a QA release
6. Once the QA release is verified, the new code is approved for a Production release

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/Ideal-Pipeline.png)

## The Issue:
If you have lots source control check-ins throughout the day, you get the above steps executing each and every time, but you really do not want to reject the code to prevent a release to QA for each check in.  Your approval person will probably get pretty annoyed.  

## What I have seen:
Customer will build two pipelines.
1. Dev: Steps 1 through 4 from above still apply, but there is no link to QA.  
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/SeperateDevRelease.png)

2. QA-Prod: Steps 5 and 6 are put in a different pipeline which needs to be kicked off.  And even worse I have the QA/Prod piepline rebuild the code.  You should only build your code once!  Building for Dev and then building for QA-Prod introduces possible issues.
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/SeperateQAProdRelease.png)

## The solution
1. Implement the Ideal pipeline
2. In Step 5 add the following:
  a. Keep our normal approval process to QA
  b. Add a Gate
  c. The Gate will wait until the Build has a Build Tag applied (this is a tag on your build, not your source control)
  d. Once the Build tag is applied the release will continue
  e. The approval to QA is performed and everything works as normal

### How to setup the build tag gate
1. Copy the Azure Function that is in this repository and deploy
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
 10. You also need to set the number of parallel deployments for the QA stage to Unlimited.  This means we will have many release pipelines attempting to release to QA at the same time.  So, once a build is tagged that release will move forward.  You have to be aware that tagging a bunch of builds at the same time would trigger many releases to QA simultaneously.

#### QA Gate approvals
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Gate-Approval-Setting.png)

#### QA Function Gate
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Gate-1.png)

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Gate-2.png)

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Gate-Approval-Setting.png)

#### QA Deployment Queue Settings 
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Change-Parallel-Deployments)

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Change-Parallel-Unlimited)

#### QA Gate failure since Build is not Tagged
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/Gate-Failed.png)

#### Tagging the Build
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/AddABuildTag.png)

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/BuildTagAdded.png)

#### QA Gate passed, waiting for approvals
![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline/master/images/QA-Waiting-For-Approval.png)
