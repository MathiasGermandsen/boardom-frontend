/**
 * Boardom guided tour using Driver.js
 * Triggered automatically on first dashboard visit (localStorage flag),
 * and manually via window.startTour() from the Blazor sidebar button.
 */

const TOUR_SEEN_KEY = 'boardom_tour_seen';

function startTour() {
  const driver = window.driver.js.driver;

  const tourInstance = driver({
    animate: true,
    smoothScroll: true,
    showProgress: true,
    progressText: '{{current}} of {{total}}',
    nextBtnText: 'Next →',
    prevBtnText: '← Back',
    doneBtnText: 'Got it!',
    onDestroyed: () => {
      localStorage.setItem(TOUR_SEEN_KEY, '1');
    },
    steps: [
      {
        // Intro step — no element (centred overlay)
        popover: {
          title: '👋 Welcome to BOARDOM',
          description:
            'This quick tour will walk you through the dashboard so you can get the most out of your home monitoring setup. You can replay it any time from the sidebar.',
          side: 'over',
          align: 'center',
        }
      },
      {
        element: '.dashboard-header',
        popover: {
          title: '📊 System Overview',
          description:
            'This section gives you a at-a-glance summary: how many devices are active, how many groups you have, and the average temperature across all sensors.',
          side: 'bottom',
          align: 'start',
        }
      },
      {
        element: '.dashboard-stats',
        popover: {
          title: '📈 Stats Cards',
          description:
            'These three cards show <strong>Active Devices</strong>, <strong>Device Groups</strong>, and <strong>Average Temperature</strong>. They update every time you load the dashboard.',
          side: 'bottom',
          align: 'start',
        }
      },
      {
        element: '.dashboard-groups',
        popover: {
          title: '🏠 Device Groups',
          description:
            'Your Arduinos are organised into groups here — for example by room. Each card shows the device name, online status, and the four latest sensor readings: light, temperature, humidity, and pressure.',
          side: 'top',
          align: 'start',
        }
      },
      {
        element: '.fab-container',
        popover: {
          title: '➕ Actions Menu',
          description:
            'Click the <strong>+</strong> button to open the actions menu. From here you can add a new Arduino device, rename or delete existing devices, and manage your groups.',
          side: 'left',
          align: 'end',
        }
      },
      {
        element: '.nav-menu',
        popover: {
          title: '🧭 Navigation',
          description:
            'Use the sidebar to navigate the app. <strong>Devices</strong> shows all devices and their status. <strong>Analytics</strong> lets you plot historical sensor data over time. <strong>Power Pi</strong> configures your electricity pricing settings.',
          side: 'right',
          align: 'start',
        }
      },
      {
        // Final step — no element (centred overlay)
        popover: {
          title: '✅ You\'re all set!',
          description:
            'That\'s the tour! If you need a refresher, click <strong>Start Tour</strong> in the sidebar at any time. Check out the <a href="/howto" style="color:#60a5fa;text-decoration:underline;">How To guide</a> for detailed setup instructions including Arduino wiring.',
          side: 'over',
          align: 'center',
        }
      }
    ]
  });

  tourInstance.drive();
}

/**
 * Called by Blazor's OnAfterRenderAsync on first render.
 * Returns true if the tour should be shown (first-time visitor).
 */
function checkAndStartTour() {
  if (!localStorage.getItem(TOUR_SEEN_KEY)) {
    // Small delay so the dashboard fully renders before highlighting elements
    setTimeout(startTour, 800);
    return true;
  }
  return false;
}

// Expose to Blazor JS interop
window.boardomTour = {
  startTour,
  checkAndStartTour
};
