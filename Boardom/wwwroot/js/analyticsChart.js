    const colors = [
        'rgba(59,125,217,0.8)',
        'rgba(255,99,132,0.8)',
        'rgba(75,192,192,0.8)',
        'rgba(255,205,86,0.8)',
        'rgba(153,102,255,0.8)',
        'rgba(255,159,64,0.8)',
        'rgba(201,203,207,0.8)',
        'rgba(54,162,235,0.8)',
        'rgba(255,0,0,0.8)',
        'rgba(0,255,0,0.8)'
    ];

    function getColor(label) {
        let hash = 0;
        for (let i = 0; i < label.length; i++) {
            hash = label.charCodeAt(i) + ((hash << 5) - hash);
        }
        return colors[Math.abs(hash) % colors.length];
    }

function renderLineChart(datasets) {
    const canvas = document.getElementById('analyticsChart');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    
    if (window._analyticsChart) {
        window._analyticsChart.destroy();
        window._analyticsChart = null;
    }

    window._analyticsChart = new Chart(ctx, {
        type: 'line',
        data: {
            datasets: datasets.map((d, index) => ({
                label: d.label,
                data: d.data,
                fill: false,
                borderColor: colors[index % colors.length],
                tension: 0.1
            }))
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    type: 'time',
                    display: true
                },
                y: {
                    display: true
                }
            }
        }
    });
}