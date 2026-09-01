# 🎮 Metropolia Welcoming Game
> [!CAUTION]
> **This README file is only a draft!**

Welcome to the **Innovation Project: Software/Game Development** repository! 

This project aims to create an interactive, web-based 2D "Welcoming Game" for new international students at [Metropolia University of Applied Sciences](https://opiskelijan.metropolia.fi/en/guidance-and-support/welcome-hub). 
Through various minigames, new students will explore Finnish student culture in a fun and engaging way!

## 🛠 Tech Stack
* **Engine:** Unity (2D Core)
* **Target Platform:** WebGL (Browser)
* **CI/CD:** GitHub Actions (GameCI) for automated WebGL builds

---

## 🚦 Repository Rules & Workflow

To keep our Unity project stable and avoid merge conflicts, we have strict rules for contributing. **Please read this carefully!**

### 1. Naming Conventions (Branch & Commits)
We use our Trello-Board IDs for tracking. 
* **Branches:** `<type>/<Trello-ID>-<short-description>`
  * *Example:* `feature/T-12-blueberry-picking`
  * *Types:* `feature` (new stuff), `bugfix` (fixing errors), `docs` (documentation), `refactor` (code cleanup)
* **Commits:** `[Trello-ID] type: Description in English`
  * *Example:* `[T-12] feat: add player collision to trees`

### 2. The Golden Workflow (How to contribute)
Direct pushes to the `main` branch are **blocked**.
1. Pick a task from Trello.
2. Create a new branch from `main` (see Naming Conventions).
3. Do your work in Unity and commit regularly.
4. Push your branch to GitHub.
5. Create a **Pull Request (PR)** into the `main` branch.
6. **Wait for a Review:** Your PR must be approved by our Senior Unity Devs (Code Owners) before it can be merged.
7. Once approved and the pipeline is green, hit merge!

---

## 🚀 Setup Instructions for Developers

### Prerequisites
1. **Git LFS:** Our repository uses Git Large File Storage for images and sounds. You **must** install [Git LFS](https://docs.github.com/en/repositories/working-with-files/managing-large-files/installing-git-large-file-storage?platform=mac) before cloning!
