var $url = "/study/studyCourseInfo";
var $urlSetProgress = $url + "/setProgress";
var $urlSetOver = $url + "/setOver";

var $urlCollection = $url + "/collection";
var $urlEvaluation = $url + "/evaluation";

var data = utils.init({
  id: utils.getQueryInt("id"),
  planId: utils.getQueryInt("planId"),
  courseInfo: null,
  videoPlayer: null,
  courseWareList: null,
  courseWareCurrent: null,
  courseUser: null,
  playing: false,
  setProgressInterval: null,
  ePageIndex: 1,
  ePageSize: 10,
  eTotal: 0,
  eList: [],
  eLoadMoreLoading: false,
  eAvg: '',
  eUser: 0,
  eStarList: null
});

var methods = {
  apiGet: function () {
    var $this = this;
    $api.get($url, { params: { planId: this.planId, courseId: this.id } }).then(function (response) {
      var res = response.data;

      $this.courseInfo = res.courseInfo;

      top.document.title = res.courseInfo.name;

      $this.courseWareList = res.courseInfo.courseWareList;
      $this.courseUser = res.courseInfo.courseUserInfo;

      if ($this.courseWareList && $this.courseWareList.length > 0) {
        var currentWare = $this.courseWareList.find(item => item.studyCurrent === true);
        $this.btnPlayWare(currentWare);
      }

    }).catch(function (error) {
      utils.error(error, { layer: true });
    }).then(function () {
      utils.loading($this, false);
    });
  },
  btnPlayWare: function (item) {
    if (this.videoPlayer && this.videoPlayer !== null) {
      this.videoPlayer.destroy(true);
    }
    this.videoPlayer = null;
    this.courseWareCurrent = item;
    setTimeout(this.courseWarePlan, 200);
  },
  courseWarePlan: function () {
    var $this = this;
    $this.$nextTick(_ => {
      $this.videoPlayer = new Player({
        el: document.querySelector('#videoPlayer'),
        url: $this.courseWareCurrent.courseWareInfo.url,
        autoplay: true,
        loop: false,
        videoInit: true,
        closeInactive: true,
        disableProgress: $this.courseInfo.state || $this.courseInfo.boolWare ? false : true,
        lastPlayTime: ($this.courseWareCurrent.currentDuration + 10) > $this.courseWareCurrent.courseWareInfo.duration ? 0 : $this.courseWareCurrent.currentDuration,
        //playbackRate: [0.5, 0.75, 1, 1.5, 2],
        //cssFullscreen: true,
        rotateFullscreen: true,
        fluid: true,
        miniplayerConfig: {
          top: 0,
          left: 0,
          width: 200,
          height: 100
        },
        poster: $this.courseInfo.coverImg
      });
      $this.videoPlayer.once('ready', () => {
        $this.videoPlayer.on('play', function () {
          $this.playing = true;
          $this.setProgressInterval = window.setInterval($this.setProgress, 5000);
        })
        $this.videoPlayer.on('pause', function () {
          $this.playing = false;
          if ($this.setProgressInterval) {
            window.clearInterval($this.setProgressInterval);
            $this.setProgressInterval = null;
          }

          $this.videoPlayer.exitMiniplayer();
        })
        $this.videoPlayer.on('ended', function () {
          $this.playing = false;
          if ($this.setProgressInterval) {
            window.clearInterval($this.setProgressInterval);
            $this.setProgressInterval = null;
          }

          $this.videoPlayer.exitMiniplayer();

          $this.setWareOver();
        })
      })
    });
  },
  setProgress: function () {
    var $this = this;
    let addProgress = 5;
    let isAdd = true;

    var playCurrent = Math.trunc(this.videoPlayer.currentTime);

    if ($this.courseWareCurrent.state === 'Yiwancheng' || $this.courseWareCurrent.state === 'Yidabiao') {
      let objWare = this.courseWareList.find(item => item.id === $this.courseWareCurrent.id)
      objWare.totalDuration += addProgress;
      objWare.currentDuration = playCurrent;

    }
    else {
      addProgress = playCurrent;
      isAdd = false;

      let objWare = this.courseWareList.find(item => item.id === $this.courseWareCurrent.id)
      objWare.totalDuration = addProgress;
      objWare.currentDuration = playCurrent;
    }

    $api.post($urlSetProgress, { id: $this.courseWareCurrent.id, progress: addProgress, isAdd: isAdd, currentDuration: playCurrent }).then(function (response) {
      var res = response.data;
      $this.courseUser.totalDuration = res.totalDuration;
    }).catch(function (error) {
      utils.error(error, { layer: true });
    }).then(function () {
    });
  },
  setWareOver: function () {
    var $this = this;
    $api.post($urlSetOver, { id: $this.courseWareCurrent.id }).then(function (response) {
      var res = response.data;
      $this.courseUser.totalDuration = res.totalDuration;
      if (res.studyOver && !$this.courseInfo.state) {
        $this.apiGet();
      }
    }).catch(function (error) {
      utils.error(error, { layer: true });
    }).then(function () {
    });
  },
  btnCollectionClick: function () {
    var $this = this;
    utils.loading(this, true);
    $api.post($urlCollection, { id: this.courseUser.id }).then(function (response) {
      var res = response.data;
      utils.success($this.courseUser.collection ? "已取消收藏" : "已收藏", { layer: true });
      $this.courseUser.collection = !$this.courseUser.collection;
    }).catch(function (error) {
      utils.error(error, { layer: true });
    }).then(function () {
      utils.loading($this, false);
    });
  },
  btnViewExamQClick: function () {
    var $this = this;
    top.utils.openLayer({
      title: false,
      closebtn: 0,
      url: utils.getExamUrl('examQuestionnairing', { id: this.courseInfo.examQuestionnaireId, planId: this.planId, courseId: this.id }),
      width: "100%",
      height: "100%",
      end: function () {
        $this.apiGet();
      }
    });
  },
  btnViewExamClick: function () {
    var $this = this;
    top.utils.openLayer({
      title: false,
      closebtn: 0,
      url: utils.getExamUrl('examPaperInfo', { id: this.courseInfo.examId, planId: this.planId, courseId: this.id }),
      width: "100%",
      height: "100%",
      end: function () {
        $this.apiGet();
      }
    });
  },
  btnViewEvaluationClick: function () {
    var $this = this;
    top.utils.openLayer({
      title: false,
      closebtn: 0,
      url: utils.getStudyUrl('studyCourseEvaluation', { courseId: this.id, planId: this.planId, eId: this.courseInfo.studyCourseEvaluationId }),
      width: "100%",
      height: "100%",
      end: function () {
        $this.apiGet();
        $this.ePageIndex = 1;
        $this.apiGetEvaluation();
      }
    });
  },
  apiGetEvaluation: function () {
    var $this = this;
    $api.get($urlEvaluation, { params: { planId: this.planId, courseId: this.id, pageIndex: this.ePageIndex, pageSize: this.ePageSize } }).then(function (response) {
      var res = response.data;
      if ($this.ePageIndex <= 1) {
        $this.eAvg = res.starAvg;
        $this.eUser = res.starUser;
        $this.eStarList = res.starList;
      }
      if (res.list && res.list.length > 0) {
        res.list.forEach(item => {
          $this.eList.push(item);
        });
      }
      $this.eTotal = res.total;

    }).catch(function (error) {
      utils.error(error, { layer: true });
    }).then(function () {
      $this.eLoadMoreLoading = false;
    });
  },
  btnLoadMoreEvaluationClick: function () {
    this.eLoadMoreLoading = true;
    this.ePageIndex++;
    this.apiGetEvaluation();
  },
  scrolling: function () {
    let currentScrollTop = document.documentElement.scrollTop || document.body.scrollTop;
    if (currentScrollTop > 250) {
      if (this.playing) {
        this.videoPlayer.getMiniplayer()
      }
    }
    else {
      if (this.playing) {
        this.videoPlayer.exitMiniplayer()
      }
    }

  }
};

var $vue = new Vue({
  el: "#main",
  data: data,
  mounted: function () {
    window.addEventListener("scroll", this.scrolling);
  },
  methods: methods,
  created: function () {
    this.apiGet();
    this.apiGetEvaluation();
  },
});
