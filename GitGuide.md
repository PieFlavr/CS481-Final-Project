## Git Guide

### Basics

Pull: ``git pull``

Check status: ``git status``<br>

Add changes: ``git add .``<br>
Add changes (specific): ``git add file_path``<br>

Commit: ``git commit -m "Message"``<br>

Push: ``git push``

### Branching

Checkout to a new branch: ``git checkout -b yourname/new_branch_name``<br>
Checkout to an existing branch: ``git checkout yourname/branch_name``<br>

Check your current branch: ``git branch -a``<br>

Delete a branch: ``git branch -d branch_name``<br>
Force delete a branch (caution): ``git branch -D branch_name``<br>

After a pull request to main:<br>
``git checkout main``<br>
``git pull --prune``<br>
``git checkout yourname/new_branch_name``<br>
``git push --set-upstream origin new_branch_name``

### Rebasing
When rebasing, git essentially asks "What would have happened if everything that had happened during ``name_of_branch_to_rebase_onto`` had happened before whatever had happened before ``yourname/working_branch``?" Because the past may have changes to code or assets that the present also interacts with, there may be conflicts. These are resolved during the rebase. Remember: you can always abort a rebase by using ``git rebase --abort``<br>
``git checkout yourname/working_branch``<br>
``git fetch origin``<br>
``git rebase name_of_branch_to_rebase_onto``<br>
``git pull --rebase``

### Extras
Pretty log: ``git log --all --decorate --oneline --graph``

### Etiquette

Naming your branches in a standard manner is convenient for the team.<br>
Please name your branch your_name_short/branch_name:<br>
``davinci/wing_refactor``<br>
``mona/security_patch``<br>
``alexz/squirrel_gui``<br>
Branch names should be easy to read and easy to type. While naming a branch ``alexz/gar`` is fun, it also doesn't convey what that branch is responsible for. Unless there's an understanding that ``gar`` is a version with certain features, a better branch name would be ``alexz/rnn_3``.

#### Why append your name to every branch you create?

First of all, appending your name to a branch isn't a declaration that nobody else can work on it. It is, however, difficult to simultaneously work on the same branch (say that both Chanel and Alyssa checkout to `chanelk/data_logger` and each commit 25 times. Either they will have to be next to each other coordinating pulls every time the other pushes, which will probably lead to merge conflicts, or there will be a huge merge conflict at the end of the day after both of them potentially worked on the same thing for the whole day). Branches are really convenient because they provide the opportunity to work without worry of overlap. If two people want to work on the same code in the same files, the work probably hasn't been divided correctly. Pair programming can be done with one person typing and the other sitting next to the first making comments with both switching positions every once in a while. There are also programs that allow multiple to work on the same file at once. If multiple people want to work on the exact same code, try and avoid merge conflicts as much as possible by doing one of the aforementioned things.<br>
Naming a branch this way gives two important pieces of information: who should be contated about the code in that branch and what purpose it serves.<br>
`alexz` tells someone checking into that branch that Alex Zorzella wrote that code, but doesn't offer any other information.<br>
`squirrel_gui` tells someone checking into that branch that there's going to be Squirrel GUI code in that branch, but they'll have to read the commit history to find the person to contact. Yes, someone looking at a branch can always check the commit history for the author, but naming the branch well will lead to good info when someone calls `git branch -a`.<br>
`alexz/squirrel_gui` tells someone checking into that branch a lot of good information.<br><br>
```
ameliaq@wpeb-436-14:~/Documents/GitHub/Capstone $ git branch -a
alexz/squirrel_gui
main
chanelk/data_logger
alyssaw/security_hotfix
jbaze/ml_pipeline
ameliaq/cat_launcher
jbaze/ml_pipeline/movement
monalisa/ml_gui
kathyj/propellant_driver *
remotes/origin/HEAD -> origin/main
remotes/origin/alexz/squirrel_gui
remotes/origin/main
remotes/origin/chanelk/data_logger
remotes/origin/alyssaw/security_hotfix
remotes/origin/jbaze/ml_pipeline
remotes/origin/ameliaq/cat_launcher
remotes/origin/jbaze/ml_pipeline/movement
remotes/origin/monalisa/ml_gui
remotes/origin/kathyj/propellant_driver
```

#### Mass Renaming

When working in the codebase, it might be tempting to rename some variables. Maybe the IDE is suggesting it, or maybe it's just cleanup. Make sure to only mass rename variables that are either solely contained in your domain or ones that will definitely not be used by others until the next merge.
The same goes for changing the parameters for a function. If there is a function that should be renamed or a function that needs new variables, the best course of action is to put a note for the next time there will be a renaming opportunity, and if the situation warrants it, writing a wrapper for the old function with the new name and/or passed variables. Writing wrappers for old functions allows you to continue working without having to wait for the next merging of everyone's branches and allows people to slowly update their code to reflect the new convention.

