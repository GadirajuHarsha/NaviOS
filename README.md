# NaviOS: Adaptive In-Car Information System for Cognitive Load Management
NaviOS is a Unity-based simulation of an intelligent car display system designed to enhance driver safety by dynamically managing cognitive load. The system adapts its user interface in real-time, prioritizing critical information and filtering out distractions based on simulated driving conditions and a real-time cognitive load index (CLI).

## Project Overview
This project's core innovation is the application of established psychological principles to automotive UI/UX design. Instead of a static display, NaviOS actively works to reduce the mental effort required of the driver, thereby improving focus and reaction times. The system demonstrates a proactive approach to safety by adjusting the information presented to the driver in response to environmental and internal factors.

## Psychological Concepts Applied
- Cognitive Load Theory: The system intelligently reduces extraneous cognitive load by decluttering the display and manages intrinsic cognitive load by simplifying complex navigation instructions during demanding situations.

- Attention Theories (Selective & Divided Attention): The UI guides the driver's focus to the most critical information, minimizing the need to divide attention between multiple competing stimuli.

- Arousal Theory (Yerkes-Dodson Law): By controlling the flow of information and filtering out non-essential data, NaviOS aims to prevent excessive driver arousal that could impair performance.

- Habituation & Alert Fatigue: Alerts are designed to be distinct and impactful, ensuring the driver doesn't become desensitized to warnings.

## Core Features & Functionality
1. Dynamic Cognitive Load Index (CLI): A core C# system simulates a numerical metric representing the driver's mental workload.

    - The CLI naturally decays over time as a baseline.

    - The baseline is dynamically capped based on the time of day (a higher cap and slower decay at night to simulate fatigue).

    - The CLI spikes in response to user-triggered events (e.g., "Sudden Obstacle," "High Traffic Density").

2. Adaptive User Interface: The UI fluidly transforms across three defined CLI thresholds to prioritize information.

    - Low Load: Displays all information (dashboard, navigation, secondary info) in a balanced layout.

    - Moderate Load: The navigation panel expands to become the primary focus, while the secondary information panel shrinks, and its non-essential content (e.g., music, weather) is hidden. The fatigue indicator remains visible.

    - High Load: The secondary panel expands significantly to display a critical, pulsing red alert. The navigation instructions become ultra-concise, and other non-essential elements are hidden to ensure maximum attention is on the warning.

3. Autonomous Driving Simulation: A simplified environment features an autonomous car that follows a predefined path. The car's speed and turning are realistically simulated, with the car slowing down for turns and accelerating on straightaways.

4. User-Triggered Events Panel: A separate UI panel allows users to manually activate simulated driving scenarios that directly influence the CLI, demonstrating how the adaptive UI responds in real-time.

## Key Components
- CognitiveLoadManager.cs: Engineered the core logic to calculate and manage the dynamic CLI, including time-of-day adaptations and event-based spikes.

- DisplayAdapter.cs: Orchestrated the entire adaptive UI system, dynamically controlling the size and content of panels, text, and alerts using LayoutElement and CanvasGroup components. Implemented smooth panel transitions and a pulsing critical alert using the DOTween library.

- DrivingSimulator.cs: Developed the autonomous vehicle movement logic, allowing it to follow a path of pre-configured waypoints with realistic speed adjustments for turns and straight sections.

- EventTriggerer.cs: Created the user control panel that provides a clean interface for triggering the CLI events and demonstrating the system's adaptive responses.

### Technologies Used
- Unity Engine

- C#

- DOTween (Unity Asset)

- TextMeshPro
